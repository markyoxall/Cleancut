# Fetch vendor static assets into wwwroot/lib
#
# Usage: run from repository root (or the presentation project folder):
#   powershell -ExecutionPolicy Bypass -File .\src\Presentation\CleanCut.BlazorWebApp\fetch-vendor-assets.ps1
#
# This script downloads minified CSS/JS and font files and places them under
# src/Presentation/CleanCut.BlazorWebApp/wwwroot/lib/* so the app serves them locally.
#
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$libRoot = Join-Path $projectRoot "wwwroot\lib"

function Ensure-Dir($path) {
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path | Out-Null
    }
}

function Download-File($url, $outFile) {
    Write-Host "Downloading $url -> $outFile"
    $outDir = Split-Path -Parent $outFile
    Ensure-Dir $outDir
    try {
        Invoke-WebRequest -Uri $url -UseBasicParsing -OutFile $outFile -ErrorAction Stop
    }
    catch {
        Write-Warning "Failed to download $url : $_"
    }
}

# Bootstrap 5.3.8
$bootstrapVersion = "5.3.8"
$bsDir = Join-Path $libRoot "bootstrap\dist"
Ensure-Dir $bsDir
Download-File "https://unpkg.com/bootstrap@$bootstrapVersion/dist/css/bootstrap.min.css" (Join-Path $bsDir "css\bootstrap.min.css")
Download-File "https://unpkg.com/bootstrap@$bootstrapVersion/dist/js/bootstrap.bundle.min.js" (Join-Path $bsDir "js\bootstrap.bundle.min.js")

# Font Awesome (free) - download modern all.min.css + webfonts
# Uses the official @fortawesome/fontawesome-free package structure
$faVersion = "7.0.1"
$faDir = Join-Path $libRoot "fontawesome"
Ensure-Dir $faDir
Download-File "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@$faVersion/css/all.min.css" (Join-Path $faDir "css\all.min.css")
# Download the webfonts referenced by all.min.css
$faFonts = @(
    "webfonts/fa-solid-900.woff2",
    "webfonts/fa-solid-900.woff",
    "webfonts/fa-solid-900.ttf",
    "webfonts/fa-regular-400.woff2",
    "webfonts/fa-regular-400.woff",
    "webfonts/fa-regular-400.ttf",
    "webfonts/fa-brands-400.woff2",
    "webfonts/fa-brands-400.woff",
    "webfonts/fa-brands-400.ttf"
)
foreach ($f in $faFonts) {
    Download-File "https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@$faVersion/$f" (Join-Path $faDir $f)
}

# Blazorise 1.8.7 (core, bootstrap5, icons.fontawesome)
$blazoriseVersion = "1.8.7"
# Blazorise core
$blazoriseDir = Join-Path $libRoot "blazorise"
Ensure-Dir $blazoriseDir
Download-File "https://unpkg.com/blazorise@$blazoriseVersion/dist/blazorise.min.css" (Join-Path $blazoriseDir "css\blazorise.min.css")
Download-File "https://unpkg.com/blazorise@$blazoriseVersion/dist/blazorise.min.js" (Join-Path $blazoriseDir "js\blazorise.min.js")

# Blazorise Bootstrap5 provider
$blazoriseBs5Dir = Join-Path $libRoot "blazorise.bootstrap5"
Ensure-Dir $blazoriseBs5Dir
Download-File "https://unpkg.com/blazorise.bootstrap5@$blazoriseVersion/dist/blazorise.bootstrap5.min.css" (Join-Path $blazoriseBs5Dir "css\blazorise.bootstrap5.min.css")
Download-File "https://unpkg.com/blazorise.bootstrap5@$blazoriseVersion/dist/blazorise.bootstrap5.min.js" (Join-Path $blazoriseBs5Dir "js\blazorise.bootstrap5.min.js")

# Blazorise FontAwesome icons
$blazoriseIconsDir = Join-Path $libRoot "blazorise.icons.fontawesome"
Ensure-Dir $blazoriseIconsDir
Download-File "https://unpkg.com/blazorise.icons.fontawesome@$blazoriseVersion/dist/blazorise.icons.fontawesome.min.js" (Join-Path $blazoriseIconsDir "js\blazorise.icons.fontawesome.min.js")

Write-Host "Done. Check files under: $libRoot" -ForegroundColor Green
