<#
.SYNOPSIS
    Fusionne un relevé de sourcing dans la base des données historiques.

.DESCRIPTION
    Un agent de sourcing ne touche jamais à `historical-figures.json` : il rend un relevé, et
    ce script l'y verse. La base est un registre de citations — sa seule valeur est qu'aucune
    ligne n'y entre sans une source qu'on peut ouvrir — donc la fusion est un contrôle avant
    d'être une copie, et elle est TOUT OU RIEN. Un relevé qui pèche sur un point est refusé
    entier : une base à moitié fusionnée serait une base dont personne ne sait plus ce qu'elle
    contient.

    Forme attendue du relevé :

        {
          "sources": [
            { "code", "organisation", "title", "url", "capture", "kind",
              "statedUpdate" (facultatif), "note" (facultatif) }
          ],
          "observations": [
            { "figure", "date", "value", "unit", "sourceCode", "confidence",
              "confidenceWhy", "why", "retained" (facultatif), "figureLabel" (voir plus bas) }
          ],
          "notFound": [ ... ]                     — repris tel quel dans le compte rendu
        }

    Ce qui fait refuser :

    - une source sans adresse, ou dont l'adresse n'est pas une URL — une citation qu'on ne peut
      pas ouvrir n'est pas une citation, c'est une affirmation ;
    - une observation qui cite un `sourceCode` qu'aucune source ne définit, ni dans la base ni
      dans le relevé lui-même — c'est exactement ce que cette base existe pour rendre
      impossible ;
    - une observation sans `sourceCode` : un chiffre que rien ne soutient reste une constante
      de scénario, écrite à la main par l'auteur, jamais versée par une fusion ;
    - un champ obligatoire absent ou vide, un `value` qui n'est pas un nombre, une confiance
      qui n'est ni Haute, ni Moyenne, ni Basse — le bandeau de confiance de la page retombe
      silencieusement sur « Basse » pour tout le reste ;
    - un même `code` de source deux fois dans le relevé.

    Ce qui ne fait PAS refuser :

    - une source dont le code existe déjà : elle est conservée telle quelle, jamais écrasée.
      La base est la référence, le relevé est la proposition ;
    - une observation déjà présente à la même date et sur la même source : elle est ignorée,
      de sorte que rejouer deux fois le même relevé ne double rien ;
    - une figure inconnue : elle est créée. Son code doit suivre la forme `<poste>-<ru|ua>` —
      c'est ainsi que le bandeau écrit ses liens, et une figure nommée autrement serait
      documentée et inatteignable — et son libellé d'affichage doit venir avec, en
      `figureLabel`, faute de quoi la page s'intitulerait d'un code.

.PARAMETER Path
    Le relevé JSON à fusionner.

.PARAMETER DatabasePath
    La base à modifier. Par défaut celle du dépôt.

.PARAMETER DryRun
    Contrôle et compte rendu, sans rien écrire.

.EXAMPLE
    scripts\Merge-Provenance.ps1 .tmp\sourcing-petrole.json

.EXAMPLE
    scripts\Merge-Provenance.ps1 .tmp\sourcing-petrole.json -DryRun
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string] $Path,

    [string] $DatabasePath,

    [switch] $DryRun
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $DatabasePath) {
    $DatabasePath = Join-Path $repoRoot "src\TheoryOfVictory.Engine\data\historical-figures.json"
}

if (-not (Test-Path -LiteralPath $Path)) {
    throw "Relevé introuvable : $Path"
}

if (-not (Test-Path -LiteralPath $DatabasePath)) {
    throw "Base introuvable : $DatabasePath"
}

$confidences = @("Haute", "Moyenne", "Basse")

# Les codes de source se comparent à la casse près : le moteur les résout dans un dictionnaire
# ordinal, donc « CBR » et « cbr » y sont deux sources, dont une seule existe.
$ordinal = [System.StringComparer]::Ordinal

# Un JsonArray et un JsonObject sont énumérables, et PowerShell déroule tout ce qu'une fonction
# renvoie d'énumérable. Sans la virgule, ces fonctions rendraient les ÉLÉMENTS au lieu du nœud.
function Read-JsonFile {
    param([string] $File)

    $node = [System.Text.Json.Nodes.JsonNode]::Parse([System.IO.File]::ReadAllText($File))
    if ($null -eq $node -or $node.GetValueKind() -ne [System.Text.Json.JsonValueKind]::Object) {
        throw "$File n'est pas un objet JSON."
    }

    return , $node
}

function Get-Text {
    param($Node, [string] $Name)

    if ($null -eq $Node) { return $null }

    $value = $Node[$Name]
    if ($null -eq $value -or $value.GetValueKind() -ne [System.Text.Json.JsonValueKind]::String) {
        return $null
    }

    $text = $value.ToString()
    if ([string]::IsNullOrWhiteSpace($text)) { return $null }

    return $text
}

function Get-Array {
    param($Node, [string] $Name)

    if ($null -eq $Node) { return $null }

    $value = $Node[$Name]
    if ($null -eq $value -or $value.GetValueKind() -ne [System.Text.Json.JsonValueKind]::Array) {
        return $null
    }

    return , $value
}

# Un nœud ne peut appartenir qu'à un seul parent : ce qui vient du relevé se recopie avant
# d'entrer dans la base. Par le texte plutôt que par DeepClone, qui n'existe pas partout.
function Copy-Node {
    param($Node)

    if ($null -eq $Node) { return $null }

    return , ([System.Text.Json.Nodes.JsonNode]::Parse($Node.ToJsonString()))
}

function Find-Figure {
    param($Figures, [string] $Name)

    foreach ($figure in $Figures) {
        if ([string]::Equals((Get-Text $figure "code"), $Name, "OrdinalIgnoreCase")) { return , $figure }
    }

    foreach ($figure in $Figures) {
        if ([string]::Equals((Get-Text $figure "label"), $Name, "OrdinalIgnoreCase")) { return , $figure }
    }

    return $null
}

$base = Read-JsonFile $DatabasePath
$intake = Read-JsonFile $Path

$baseSources = Get-Array $base "sources"
$baseFigures = Get-Array $base "figures"
if ($null -eq $baseSources -or $null -eq $baseFigures) {
    throw "La base $DatabasePath n'a pas ses deux tableaux « sources » et « figures »."
}

$knownSources = [System.Collections.Generic.HashSet[string]]::new($ordinal)
foreach ($source in $baseSources) {
    $code = Get-Text $source "code"
    if ($code) { [void] $knownSources.Add($code) }
}

$refusals = [System.Collections.Generic.List[string]]::new()

# ---------------------------------------------------------------------------------------------
# Les sources d'abord : une observation ne se contrôle qu'une fois connues les sources que le
# relevé apporte lui-même.
# ---------------------------------------------------------------------------------------------

$sourcesToAdd = [System.Collections.Generic.List[object]]::new()
$sourcesKept = [System.Collections.Generic.List[string]]::new()
$seenInIntake = [System.Collections.Generic.HashSet[string]]::new($ordinal)

$intakeSources = Get-Array $intake "sources"
$rank = 0

foreach ($source in $intakeSources) {
    $rank++

    if ($null -eq $source -or $source.GetValueKind() -ne [System.Text.Json.JsonValueKind]::Object) {
        $refusals.Add("Source n°$rank : ce n'est pas un objet.")
        continue
    }

    $code = Get-Text $source "code"
    if (-not $code) {
        $refusals.Add("Source n°$rank : « code » manquant.")
        continue
    }

    $missing = @()
    foreach ($field in @("organisation", "title", "url", "capture", "kind")) {
        if (-not (Get-Text $source $field)) { $missing += $field }
    }

    if ($missing.Count -gt 0) {
        $refusals.Add("Source « $code » : champ(s) obligatoire(s) manquant(s) — $($missing -join ", ").")
    }

    $url = Get-Text $source "url"
    if ($url -and $url -notmatch "^https?://") {
        $refusals.Add("Source « $code » : « $url » n'est pas une adresse ouvrable.")
    }

    if (-not $seenInIntake.Add($code)) {
        $refusals.Add("Source « $code » : déclarée deux fois dans le relevé.")
        continue
    }

    if ($knownSources.Contains($code)) {
        # La base gagne, toujours : une source déjà citée par des observations existantes ne
        # peut pas changer de contenu sur la foi d'un relevé.
        $sourcesKept.Add($code)
        continue
    }

    $sourcesToAdd.Add([pscustomobject]@{ Code = $code; Node = (Copy-Node $source) })
    [void] $knownSources.Add($code)
}

# ---------------------------------------------------------------------------------------------
# Les observations, rattachées à leur figure — existante ou à créer.
# ---------------------------------------------------------------------------------------------

$observationsToAdd = [System.Collections.Generic.List[object]]::new()
$observationsKnown = [System.Collections.Generic.List[string]]::new()
$figuresToCreate = [ordered] @{}

$intakeObservations = Get-Array $intake "observations"
$rank = 0

foreach ($observation in $intakeObservations) {
    $rank++

    if ($null -eq $observation -or $observation.GetValueKind() -ne [System.Text.Json.JsonValueKind]::Object) {
        $refusals.Add("Observation n°$rank : ce n'est pas un objet.")
        continue
    }

    $figureName = Get-Text $observation "figure"
    $date = Get-Text $observation "date"
    $label = if ($figureName -and $date) { "$figureName / $date" } else { "n°$rank" }

    $missing = @()
    foreach ($field in @("figure", "date", "unit", "sourceCode", "confidence", "confidenceWhy", "why")) {
        if (-not (Get-Text $observation $field)) { $missing += $field }
    }

    if ($missing.Count -gt 0) {
        $refusals.Add("Observation $label : champ(s) obligatoire(s) manquant(s) — $($missing -join ", ").")
        continue
    }

    $value = $observation["value"]
    if ($null -eq $value -or $value.GetValueKind() -ne [System.Text.Json.JsonValueKind]::Number) {
        $refusals.Add("Observation $label : « value » doit être un nombre.")
    }

    $confidence = Get-Text $observation "confidence"
    if ($confidences -cnotcontains $confidence) {
        $refusals.Add("Observation $label : confiance « $confidence » — attendu Haute, Moyenne ou Basse.")
    }

    $sourceCode = Get-Text $observation "sourceCode"
    if (-not $knownSources.Contains($sourceCode)) {
        $refusals.Add("Observation $label : cite la source « $sourceCode », qu'aucune source ne définit — ni la base, ni le relevé.")
    }

    $retained = $observation["retained"]
    if ($null -ne $retained -and
        $retained.GetValueKind() -ne [System.Text.Json.JsonValueKind]::True -and
        $retained.GetValueKind() -ne [System.Text.Json.JsonValueKind]::False) {
        $refusals.Add("Observation $label : « retained » doit être vrai ou faux.")
    }

    $figure = Find-Figure $baseFigures $figureName

    if ($null -eq $figure -and -not $figuresToCreate.Contains($figureName)) {
        $figureLabel = Get-Text $observation "figureLabel"

        if ($figureName -cnotmatch "^[a-z0-9]+(-[a-z0-9]+)*-(ru|ua)$") {
            $refusals.Add("Figure « $figureName » : inconnue, et son code ne suit pas la forme <poste>-<ru|ua> par laquelle le bandeau écrit ses liens.")
        }
        elseif (-not $figureLabel) {
            $refusals.Add("Figure « $figureName » : inconnue, et son libellé manque — ajoute « figureLabel » à la première observation qui la nomme.")
        }
        else {
            $figuresToCreate[$figureName] = [pscustomobject]@{
                Code = $figureName
                Label = $figureLabel
                Unit = (Get-Text $observation "unit")
                EngineSide = $(if ($figureName -cmatch "-ru$") { "invader" } else { "defender" })
                EnginePost = $figureName.Substring(0, $figureName.Length - 3)
            }
        }
    }

    if ($null -ne $figure) {
        $existingObservations = Get-Array $figure "observations"
        $twin = $false

        foreach ($existing in $existingObservations) {
            if ((Get-Text $existing "date") -ceq $date -and (Get-Text $existing "sourceCode") -ceq $sourceCode) {
                $twin = $true
                break
            }
        }

        if ($twin) {
            $observationsKnown.Add($label)
            continue
        }
    }

    $observationsToAdd.Add([pscustomobject]@{ Figure = $figureName; Label = $label; Node = $observation })
}

# ---------------------------------------------------------------------------------------------
# Le refus, s'il y a lieu — avant la moindre écriture.
# ---------------------------------------------------------------------------------------------

if ($refusals.Count -gt 0) {
    Write-Host ""
    Write-Host "Fusion refusée — $($refusals.Count) point(s), la base n'a pas été touchée :"
    foreach ($refusal in $refusals) {
        Write-Host "  · $refusal"
    }

    Write-Host ""
    exit 1
}

# ---------------------------------------------------------------------------------------------
# L'application.
# ---------------------------------------------------------------------------------------------

foreach ($entry in $sourcesToAdd) {
    $baseSources.Add($entry.Node)
}

foreach ($entry in $figuresToCreate.Values) {
    $created = [System.Text.Json.Nodes.JsonNode]::Parse((
        [ordered] @{
            code = $entry.Code
            label = $entry.Label
            unit = $entry.Unit
            engineSide = $entry.EngineSide
            enginePost = $entry.EnginePost
            observations = @()
        } | ConvertTo-Json -Depth 4))

    $baseFigures.Add($created)
}

$addedByFigure = [ordered] @{}

foreach ($entry in $observationsToAdd) {
    $figure = Find-Figure $baseFigures $entry.Figure
    if ($null -eq $figure) {
        throw "Figure « $($entry.Figure) » introuvable après création : la fusion s'arrête sans écrire."
    }

    # Reconstruite champ par champ plutôt que recopiée : le relevé peut porter des clés qui ne
    # sont pas du schéma, et la base ne doit contenir que ce qu'elle sait relire.
    $written = [System.Text.Json.Nodes.JsonObject]::new()
    foreach ($field in @("date", "value", "unit", "sourceCode", "confidence", "confidenceWhy")) {
        $written[$field] = Copy-Node $entry.Node[$field]
    }

    $retained = $entry.Node["retained"]
    if ($null -eq $retained) {
        # Une observation versée par une fusion ne se déclare pas d'elle-même valeur du moteur :
        # ce que le moteur porte se décide en calibrant, pas en sourçant.
        $written["retained"] = [System.Text.Json.Nodes.JsonNode]::Parse("false")
    }
    else {
        $written["retained"] = Copy-Node $retained
    }

    $written["why"] = Copy-Node $entry.Node["why"]

    $observations = Get-Array $figure "observations"
    $observations.Add($written)

    $code = Get-Text $figure "code"
    if (-not $addedByFigure.Contains($code)) { $addedByFigure[$code] = 0 }
    $addedByFigure[$code] = $addedByFigure[$code] + 1
}

# Dernier contrôle sur le RÉSULTAT, et non sur le relevé : ce qui s'écrit doit se charger. Le
# moteur lève à l'ouverture sur une observation orpheline, et un fichier qu'il refuse de lire
# éteindrait le site entier.
$finalSources = [System.Collections.Generic.HashSet[string]]::new($ordinal)
foreach ($source in $baseSources) {
    $code = Get-Text $source "code"
    if (-not (Get-Text $source "url")) {
        throw "La base fusionnée porterait la source « $code » sans adresse : rien n'a été écrit."
    }

    if (-not $finalSources.Add($code)) {
        throw "La base fusionnée porterait deux fois la source « $code » : rien n'a été écrit."
    }
}

foreach ($figure in $baseFigures) {
    $observations = Get-Array $figure "observations"
    foreach ($observation in $observations) {
        $cited = Get-Text $observation "sourceCode"
        if ($cited -and -not $finalSources.Contains($cited)) {
            throw "La base fusionnée citerait la source inconnue « $cited » : rien n'a été écrit."
        }
    }
}

if (-not $DryRun) {
    $options = [System.Text.Json.JsonSerializerOptions]::new()
    $options.WriteIndented = $true
    # Sans cet encodeur, chaque accent partirait en é : le fichier resterait valide et
    # deviendrait illisible, alors qu'il se relit à la main plus souvent qu'il ne se charge.
    $options.Encoder = [System.Text.Encodings.Web.JavaScriptEncoder]::UnsafeRelaxedJsonEscaping

    $json = ($base.ToJsonString($options) -replace "`r`n", "`n") + "`n"
    [System.IO.File]::WriteAllText($DatabasePath, $json, [System.Text.UTF8Encoding]::new($false))
}

# ---------------------------------------------------------------------------------------------
# Le compte rendu.
# ---------------------------------------------------------------------------------------------

$mode = if ($DryRun) { "   (contrôle seul, rien n'a été écrit)" } else { "" }

Write-Host ""
Write-Host "Relevé : $Path"
Write-Host "Base   : $DatabasePath$mode"

Write-Host ""
Write-Host "Sources"
foreach ($entry in $sourcesToAdd) { Write-Host "  + $($entry.Code)" }
foreach ($code in $sourcesKept) { Write-Host "  = $code — déjà dans la base, conservée telle quelle" }
if ($sourcesToAdd.Count -eq 0 -and $sourcesKept.Count -eq 0) { Write-Host "  aucune" }

Write-Host ""
Write-Host "Chiffres"
foreach ($entry in $figuresToCreate.Values) { Write-Host "  * $($entry.Code) « $($entry.Label) » — figure créée" }
foreach ($code in $addedByFigure.Keys) { Write-Host "  + $code — $($addedByFigure[$code]) observation(s)" }
foreach ($label in $observationsKnown) { Write-Host "  = $label — déjà présente, ignorée" }
if ($addedByFigure.Count -eq 0 -and $observationsKnown.Count -eq 0) { Write-Host "  aucun" }

$notFound = Get-Array $intake "notFound"
if ($null -ne $notFound -and $notFound.Count -gt 0) {
    Write-Host ""
    Write-Host "Non trouvé par le relevé — rien n'entre dans la base pour ces points :"
    foreach ($item in $notFound) {
        if ($null -ne $item) { Write-Host "  · $($item.ToString())" }
    }
}

Write-Host ""
