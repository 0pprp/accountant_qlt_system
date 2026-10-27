#Requires -Version 5.1
<#
.SYNOPSIS
  Builds the Vue admin panel into publish-out/panel.
#>
$ErrorActionPreference = 'Stop'
$Root = Resolve-Path (Join-Path $PSScriptRoot '..')
$PanelDir = Join-Path $Root 'panel-admin-main\panel-admin-main'
$OutDir = Join-Path $Root 'publish-out\panel'

if (-not (Test-Path $PanelDir)) {
    throw "Panel project not found: $PanelDir"
}

Push-Location $PanelDir
try {
    if (Get-Command yarn -ErrorAction SilentlyContinue) {
        Write-Host "==> yarn install" -ForegroundColor Cyan
        yarn install --frozen-lockfile
        if ($LASTEXITCODE -ne 0) { yarn install }
        Write-Host "==> yarn build" -ForegroundColor Cyan
        yarn build-only
    } else {
        Write-Host "==> npm ci / build" -ForegroundColor Cyan
        if (Test-Path 'package-lock.json') { npm ci } else { npm install }
        npm run build-only
    }
    if ($LASTEXITCODE -ne 0) { throw "panel build failed ($LASTEXITCODE)" }

    $dist = Join-Path $PanelDir 'dist'
    if (-not (Test-Path $dist)) { throw "dist not found after build" }

    if (Test-Path $OutDir) { Remove-Item -Recurse -Force $OutDir }
    New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
    Copy-Item -Recurse -Force (Join-Path $dist '*') $OutDir
    Write-Host "Panel published -> $OutDir" -ForegroundColor Green
}
finally {
    Pop-Location
}
