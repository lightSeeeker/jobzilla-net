# Syncs the original Jobzilla HTML template from the repo root into the ASP.NET Core wwwroot folders.
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$webRoot = Join-Path $RepoRoot "src\Jobzilla.Web\wwwroot"
$templateRoot = Join-Path $webRoot "template"

if (-not (Test-Path $webRoot)) {
    throw "Web root not found: $webRoot"
}

New-Item -ItemType Directory -Force -Path $templateRoot | Out-Null

Get-ChildItem -Path $RepoRoot -Filter "*.html" -File | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination (Join-Path $templateRoot $_.Name) -Force
}

$assetFolders = @("css", "js", "images", "fonts", "files", "img", "Trident")
foreach ($folder in $assetFolders) {
    $source = Join-Path $RepoRoot $folder
    if (-not (Test-Path $source)) {
        continue
    }

    $wwwTarget = Join-Path $webRoot $folder
    $templateTarget = Join-Path $templateRoot $folder

    New-Item -ItemType Directory -Force -Path $wwwTarget, $templateTarget | Out-Null
    Copy-Item -Path (Join-Path $source "*") -Destination $wwwTarget -Recurse -Force
    Copy-Item -Path (Join-Path $source "*") -Destination $templateTarget -Recurse -Force
}

$htmlCount = (Get-ChildItem -Path $templateRoot -Filter "*.html" -File).Count
Write-Host "Synced $htmlCount HTML modules and static assets to $webRoot"
