#Requires -Version 5.1
<#
.SYNOPSIS
  Publishes the ASP.NET API to publish-out/api (Release).
#>
$ErrorActionPreference = 'Stop'
$Root = Resolve-Path (Join-Path $PSScriptRoot '..')
$ApiCsproj = Join-Path $Root 'api-main\api-main\src\Api\Api.csproj'
$OutDir = Join-Path $Root 'publish-out\api'

if (-not (Test-Path $ApiCsproj)) {
    throw "Api project not found: $ApiCsproj"
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
Write-Host "==> Publishing API -> $OutDir" -ForegroundColor Cyan
dotnet publish $ApiCsproj -c Release -o $OutDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }
Write-Host "API published." -ForegroundColor Green
