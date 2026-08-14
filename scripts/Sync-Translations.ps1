<#
.SYNOPSIS
    Relit les appels de traduction du code et met les deux catalogues à jour.

.DESCRIPTION
    La clé EST le texte français source — `Localizer.Loc("Réserves")` en C# et dans les vues,
    `T("Réserves")` dans le JavaScript. Ce script relève ces clés là où elles sont écrites, et
    écrit :

      · ui.fr.json — l'INVENTAIRE de ce qui doit être traduit. La valeur y est identique à la
        clé : ce n'est pas une traduction, c'est la liste. Un test refuse qu'elle en diverge,
        parce qu'une valeur française différente de sa clé ferait mentir le code.
      · ui.en.json — la traduction. Les valeurs déjà écrites sont conservées, les clés
        disparues sont retirées, les nouvelles arrivent vides pour être traduites à la main.

    Une clé vide en anglais fait échouer le test du filet : elle ne peut pas partir en ligne.

.EXAMPLE
    scripts\Sync-Translations.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$catalogueDir = Join-Path $repoRoot "src\TheoryOfVictory.Core\i18n"

# Les trois façons d'appeler le catalogue, et rien d'autre. Une chaîne qui n'entre pas par là
# n'est pas traduisible, et c'est le test des chaînes accentuées qui la rattrape.
#
# La clé peut s'écrire sur plusieurs lignes, collée par des + : le compilateur n'en fait qu'une
# chaîne, et l'inventaire doit la recomposer de la même façon — sans quoi il enregistrerait le
# premier tiers d'une phrase, que le code ne demandera jamais.
$literal = '"(?:[^"\\]|\\.)*"'
$joined = "(?<parts>$literal(?:\s*\+\s*$literal)*)"

$patterns = @(
    "Localizer\.Loc\(\s*$joined",
    "(?<![\w.])T\(\s*$joined",
    "tov\.t\(\s*$joined"
)

function Get-SourceFiles {
    $roots = @(
        (Join-Path $repoRoot "src"),
        (Join-Path $repoRoot "tests")
    )

    Get-ChildItem -Path $roots -Recurse -File -Include *.cs, *.cshtml, *.js |
        Where-Object {
            $_.FullName -notmatch '\\(obj|bin|lib)\\' -and $_.Name -ne "i18n.js"
        }
}

function Convert-FromLiteral([string] $value) {
    # Le littéral porte ses échappements ; le catalogue porte le texte.
    return $value.Replace('\"', '"').Replace('\\', '\').Replace('\n', "`n")
}

$keys = [System.Collections.Generic.SortedSet[string]]::new([StringComparer]::Ordinal)

foreach ($file in Get-SourceFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    foreach ($pattern in $patterns) {
        foreach ($match in [regex]::Matches($content, $pattern)) {
            $key = ""
            foreach ($piece in [regex]::Matches($match.Groups["parts"].Value, $literal)) {
                $key += Convert-FromLiteral $piece.Value.Trim('"')
            }

            if ($key.Trim().Length -gt 0) {
                [void] $keys.Add($key)
            }
        }
    }
}

function Read-Catalogue([string] $path) {
    # Ordinal, jamais le comparateur par défaut de PowerShell : « Basse » et « basse » sont deux
    # étiquettes distinctes du jeu, et une table insensible à la casse en perdrait une en silence.
    $table = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
    if (-not (Test-Path $path)) { return $table }

    # -AsHashtable : « Basse » et « basse » sont deux étiquettes du jeu, et l'objet PowerShell
    # par défaut refuse de porter les deux.
    $json = Get-Content -Path $path -Raw -Encoding UTF8 | ConvertFrom-Json -AsHashtable
    foreach ($key in $json.Keys) {
        $table[$key] = $json[$key]
    }
    return $table
}

function Write-Catalogue([string] $path, [string] $comment, $entries) {
    # Ordinal ici aussi : [ordered] @{} confondrait « Basse » et « basse » à l'écriture.
    $ordered = [System.Collections.Specialized.OrderedDictionary]::new([StringComparer]::Ordinal)
    $ordered["_comment"] = $comment
    foreach ($key in ($entries.Keys | Sort-Object -CaseSensitive)) {
        $ordered[$key] = $entries[$key]
    }

    # UTF-8 sans BOM : le fichier est lu par System.Text.Json, qui accepte les deux, et par des
    # yeux humains, qui n'ont pas à voir trois octets parasites en tête de ligne.
    $json = ($ordered | ConvertTo-Json -Depth 3)
    [System.IO.File]::WriteAllText($path, $json + "`n", [System.Text.UTF8Encoding]::new($false))
}

$frenchPath = Join-Path $catalogueDir "ui.fr.json"
$englishPath = Join-Path $catalogueDir "ui.en.json"

function Get-Comment($table) {
    $comment = $null
    [void] $table.TryGetValue("_comment", [ref] $comment)
    return $comment
}

$frenchComment = Get-Comment (Read-Catalogue $frenchPath)
$englishBefore = Read-Catalogue $englishPath
$englishComment = Get-Comment $englishBefore


$french = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
$english = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
$missing = @()

foreach ($key in $keys) {
    $french[$key] = $key
    $existing = $null
    [void] $englishBefore.TryGetValue($key, [ref] $existing)
    if ([string]::IsNullOrWhiteSpace($existing)) {
        $english[$key] = ""
        $missing += $key
    }
    else {
        $english[$key] = $existing
    }
}

$dropped = @($englishBefore.Keys | Where-Object { $_ -ne "_comment" -and -not $keys.Contains($_) })

Write-Catalogue $frenchPath $frenchComment $french
Write-Catalogue $englishPath $englishComment $english

Write-Host "$($keys.Count) clés relevées dans le code."
Write-Host "$($missing.Count) sans traduction anglaise, $($dropped.Count) devenues orphelines et retirées."

foreach ($key in $missing) {
    Write-Host "  à traduire : $key"
}
