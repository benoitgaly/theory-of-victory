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

.PARAMETER OutputPath
    Répertoire de sortie. Par défaut « .artifacts/site » à la racine du dépôt.

.PARAMETER Port
    Port du serveur temporaire.
#>
[CmdletBinding()]
param(
    [string] $OutputPath,
    [int] $Port = 5399
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
            $page = Invoke-WebRequest -Uri "http://localhost:$Port/" -UseBasicParsing -TimeoutSec 20
            break
        }
        catch {
            $page = $null
        }
    }

    if ($null -eq $page) {
        throw "Le serveur n'a pas répondu sur le port $Port."
    }

    # Les actifs sont référencés en absolu ; GitHub Pages sert depuis /<dépôt>/, donc en relatif.
    $html = $page.Content -replace '(?<=(?:src|href)=")/(css/|js/|favicon)', '$1'

    Set-Content -Path (Join-Path $OutputPath "index.html") -Value $html -Encoding UTF8

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
