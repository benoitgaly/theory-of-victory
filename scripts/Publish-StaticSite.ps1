<#
.SYNOPSIS
    Fige le jeu en site statique, publiable sur GitHub Pages.

.DESCRIPTION
    La V1.0 est entièrement déterministe : les trois parties sont jouées au démarrage du
    serveur, puis rendues dans la page. Rien n'est calculé à la demande, aucun appel n'est
    fait au serveur après le chargement. Une capture de la page rendue est donc le jeu
    complet, et un hébergement statique suffit.

    Le script démarre l'application, récupère la page, réécrit les chemins d'actifs en
    relatif — GitHub Pages sert le site depuis un sous-répertoire — puis copie les feuilles
    de style et les scripts.

    Le site est BILINGUE : /fr/ et /en/ sont deux sites complets, le plateau et ses seize
    pages de provenance chacun, sous les mêmes noms de fichiers. À la racine, un renvoi par
    nom de fichier envoie le lecteur vers sa langue — celle qu'il a choisie s'il en a choisi
    une, celle de son navigateur sinon — en emportant le déroulé et le trimestre de
    l'adresse. Une adresse partagée l'an dernier continue donc d'ouvrir la même page.

.PARAMETER OutputPath
    Répertoire de sortie. Par défaut « .artifacts/site » à la racine du dépôt.

.PARAMETER Port
    Port du serveur temporaire.

.PARAMETER Deploy
    Pousse le site produit sur la branche `gh-pages`, d'où GitHub Pages le sert.

.PARAMETER Message
    Message du commit de publication. « Publication » par défaut.

.EXAMPLE
    scripts\Publish-StaticSite.ps1 -Deploy -Message "Publication — le capital en dollars"
#>
[CmdletBinding()]
param(
    [string] $OutputPath,
    [int] $Port = 5399,
    [switch] $Deploy,
    [string] $Message
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$webProject = Join-Path $repoRoot "src\TheoryOfVictory.Web\TheoryOfVictory.Web.csproj"
$wwwroot = Join-Path $repoRoot "src\TheoryOfVictory.Web\wwwroot"

if (-not $OutputPath) {
    $OutputPath = Join-Path $repoRoot ".artifacts\site"
}

if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
}
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

Write-Host "Démarrage du serveur sur le port $Port..."
$server = Start-Process -FilePath "dotnet" `
    -ArgumentList @("run", "--project", $webProject, "--urls", "http://localhost:$Port") `
    -PassThru -WindowStyle Hidden

try {
    $page = $null
    # La première requête attend la compilation puis le calcul des trois parties.
    foreach ($attempt in 1..60) {
        Start-Sleep -Seconds 2
        try {
            $page = Invoke-WebRequest -Uri "http://localhost:$Port/fr/" -UseBasicParsing -TimeoutSec 20
            break
        }
        catch {
            $page = $null
        }
    }

    if ($null -eq $page) {
        throw "Le serveur n'a pas répondu sur le port $Port."
    }

    # Les pages de provenance — une par chiffre du bandeau, plus leur sommaire. Elles sont
    # rendues par le serveur, donc elles doivent être figées comme le plateau.
    #
    # La liste n'est pas écrite ici : elle est LUE sur le sommaire, qui l'énumère déjà. Un
    # inventaire recopié dans ce script serait une deuxième vérité à maintenir, et il
    # divergerait au premier chiffre ajouté au registre.
    $pagesToFreeze = @("index.html", "provenance.html")
    $summary = Invoke-WebRequest -Uri "http://localhost:$Port/fr/provenance.html" -UseBasicParsing -TimeoutSec 30
    $pagesToFreeze += ([regex]::Matches($summary.Content, 'href="(provenance-[^"]+\.html)"') |
        ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)

    # Le sommaire n'énumère que les chiffres DOCUMENTÉS, et le bandeau lie ses sept postes des
    # deux camps, documentés ou non : un poste sans source mène à une page qui dit qu'il n'en a
    # pas, et cette page-là est la promesse du plateau autant que l'autre. Sans elle, le lien
    # tombe sur un 404 en ligne alors qu'il répond en développement.
    #
    # Les codes sont relevés sur le fichier qui écrit les liens — les recopier ici en ferait une
    # deuxième vérité, qui divergerait au premier poste renommé.
    $posts = [regex]::Match(
        [System.IO.File]::ReadAllText((Join-Path $wwwroot "js\capital.js")),
        'var POSTS = \[(?<body>.*?)\];',
        [System.Text.RegularExpressions.RegexOptions]::Singleline)

    if (-not $posts.Success) {
        throw "Les postes du bandeau ne se lisent plus dans capital.js : la liste des pages à figer serait incomplète."
    }

    foreach ($match in [regex]::Matches($posts.Groups["body"].Value, 'code:\s*"(?<code>[^"]+)"')) {
        foreach ($side in @("ru", "ua")) {
            $pagesToFreeze += "provenance-$($match.Groups['code'].Value)-$side.html"
        }
    }

    $pagesToFreeze = @($pagesToFreeze | Sort-Object -Unique)

    # Les langues sont LUES sur le sélecteur du plateau plutôt qu'écrites ici : une troisième
    # langue ajoutée au moteur doit se publier sans qu'on pense à revenir dans ce script.
    $languages = @([regex]::Matches($page.Content, 'data-lang="(?<code>[a-z]{2})"') |
        ForEach-Object { $_.Groups["code"].Value })
    $languages += "fr"
    $languages = @($languages | Sort-Object -Unique)

    foreach ($language in $languages) {
        $folder = Join-Path $OutputPath $language
        New-Item -ItemType Directory -Force -Path $folder | Out-Null

        foreach ($name in $pagesToFreeze) {
            $rendered = Invoke-WebRequest -Uri "http://localhost:$Port/$language/$name" -UseBasicParsing -TimeoutSec 30

            # Les actifs sont référencés en absolu, et les feuilles comme les scripts sont
            # partagés par les deux langues : une page vit un cran plus bas que la racine du
            # site, donc elle les remonte. GitHub Pages sert depuis /<dépôt>/, où un chemin
            # absolu pointerait hors du site.
            $frozen = $rendered.Content -replace '(?<=(?:src|href)=")/(css/|js/|favicon)', '../$1'
            Set-Content -Path (Join-Path $folder $name) -Value $frozen -Encoding UTF8
        }

        Write-Host "$($pagesToFreeze.Count) pages figées en $language."
    }

    # À la racine, une page par nom de fichier, qui renvoie vers la langue du lecteur. Elle
    # n'existe pas pour l'accueil seulement : une adresse partagée l'an dernier pointe sur
    # provenance-civilian-ru.html À LA RACINE, et elle doit continuer d'ouvrir cette page-là.
    # Le paramètre de déroulé et de trimestre voyage avec, sinon un lien partagé perdrait en
    # route ce qu'il désignait.
    $redirect = @'
<!DOCTYPE html>
<html lang="fr">
<head>
<meta charset="utf-8" />
<title>Theory of Victory</title>
<link rel="icon" href="favicon.svg" type="image/svg+xml" />
<script>
(function () {
    // Le choix déjà fait l'emporte sur la langue du navigateur : un lecteur qui a désigné une
    // langue ne doit pas être rebasculé dans l'autre au retour suivant.
    var stored = null;
    try { stored = localStorage.getItem("tov-lang"); } catch (ignored) { stored = null; }

    var known = __LANGUAGES__;
    var chosen = null;
    if (known.indexOf(stored) >= 0) { chosen = stored; }

    if (!chosen) {
        var offered = (navigator.languages || [navigator.language || "fr"]);
        for (var i = 0; i < offered.length && !chosen; i++) {
            var code = String(offered[i]).slice(0, 2).toLowerCase();
            if (known.indexOf(code) >= 0) { chosen = code; }
        }
    }

    location.replace((chosen || "fr") + "/__PAGE__" + location.search + location.hash);
})();
</script>
</head>
<body>
<p><a href="fr/__PAGE__">Le plateau, en français</a> · <a href="en/__PAGE__">The board, in English</a></p>
</body>
</html>
'@

    $languageList = "[" + (($languages | ForEach-Object { '"' + $_ + '"' }) -join ", ") + "]"

    foreach ($name in $pagesToFreeze) {
        $body = $redirect.Replace("__LANGUAGES__", $languageList).Replace("__PAGE__", $name)
        Set-Content -Path (Join-Path $OutputPath $name) -Value $body -Encoding UTF8
    }

    Write-Host "$($pagesToFreeze.Count) pages de renvoi écrites à la racine."

    foreach ($folder in @("css", "js")) {
        Copy-Item -Recurse -Force (Join-Path $wwwroot $folder) (Join-Path $OutputPath $folder)
    }

    Copy-Item -Force (Join-Path $wwwroot "favicon.svg") (Join-Path $OutputPath "favicon.svg")

    # Sans ce fichier, Pages passe le site à Jekyll, qui ignore ce qu'il ne comprend pas.
    New-Item -ItemType File -Force -Path (Join-Path $OutputPath ".nojekyll") | Out-Null

    $size = [math]::Round((Get-ChildItem -Recurse $OutputPath | Measure-Object -Property Length -Sum).Sum / 1MB, 2)
    Write-Host "Site écrit dans $OutputPath ($size Mo)."
}
finally {
    if ($server -and -not $server.HasExited) {
        Stop-Process -Id $server.Id -Force
    }

    # `dotnet run` lance l'application dans un processus fils, qui survit à son parent. On
    # n'arrête que celui qui écoute notre port : un autre serveur du même nom peut tourner
    # sur une autre copie de travail, et le tuer par son nom couperait le travail d'autrui.
    $listener = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty OwningProcess
    if ($listener) {
        Stop-Process -Id $listener -Force -ErrorAction SilentlyContinue
    }
}

# Le site en ligne est le contenu de la branche `gh-pages`, servie telle quelle par GitHub
# Pages. On y publie depuis une copie de travail jetable plutôt qu'en changeant de branche :
# la copie principale n'est jamais touchée, et un travail en cours ne peut pas partir en
# ligne par accident.
if ($Deploy) {
    if (-not $Message) {
        $Message = "Publication"
    }

    $pages = Join-Path $repoRoot ".artifacts\pages"

    if (Test-Path $pages) {
        git -C $repoRoot worktree remove $pages --force 2>&1 | Out-Null
    }

    git -C $repoRoot fetch origin gh-pages
    git -C $repoRoot worktree add $pages gh-pages
    if ($LASTEXITCODE -ne 0) {
        throw "Impossible de préparer la copie de publication."
    }

    try {
        # La branche ne contient que le site : ce qui a disparu de la sortie doit disparaître
        # en ligne, donc on remplace au lieu de superposer. Tout sauf `.git`, évidemment.
        Get-ChildItem -Path $pages -Force |
            Where-Object { $_.Name -ne ".git" } |
            Remove-Item -Recurse -Force

        Copy-Item -Path (Join-Path $OutputPath "*") -Destination $pages -Recurse -Force
        Copy-Item -Path (Join-Path $OutputPath ".nojekyll") -Destination $pages -Force

        git -C $pages add -A
        git -C $pages commit -m $Message
        git -C $pages push origin gh-pages
        if ($LASTEXITCODE -ne 0) {
            throw "La publication n'est pas partie."
        }

        Write-Host "Publié. Le site est en ligne d'ici une à deux minutes :"
        Write-Host "  https://benoitgaly.github.io/theory-of-victory/"
    }
    finally {
        git -C $repoRoot worktree remove $pages --force 2>&1 | Out-Null
    }
}
