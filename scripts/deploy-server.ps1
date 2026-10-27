#Requires -Version 5.1
<#
.SYNOPSIS
  Pulls development on the live Contabo host and rebuilds qalaat-api + qalaat_view.
  Password is NOT stored in git — pass via -RootPassword or env QALAAT_ROOT_PASSWORD.
#>
param(
    [string]$HostName = '161.97.83.160',
    [string]$RootPassword = $env:QALAAT_ROOT_PASSWORD,
    [string]$Branch = 'development'
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RootPassword)) {
    throw 'Set -RootPassword or env QALAAT_ROOT_PASSWORD (never commit the password).'
}

$askpass = Join-Path $env:TEMP ("q_askpass_{0}.cmd" -f [guid]::NewGuid().ToString('N'))
@(
    '@echo off'
    "echo $RootPassword"
) | Set-Content -Path $askpass -Encoding ascii

$env:DISPLAY = 'dummy:0'
$env:SSH_ASKPASS = $askpass
$env:SSH_ASKPASS_REQUIRE = 'force'

# LF-only bash to avoid CRLF breaking `set -o pipefail`
$bash = @"
#!/bin/bash
set -eu
cd /opt/qalaat/source
git fetch origin $Branch
git checkout $Branch
git pull origin $Branch
TAG=`$(git rev-parse --short HEAD)
echo TAG=`$TAG

cd /opt/qalaat/source/api-main/api-main
docker build -f Dockerfile.deploy -t "qalaat-api:`$TAG" -t qalaat-api:new .
cd /home/user
docker compose up -d --force-recreate qalaat-api

cd /opt/qalaat/source/panel-admin-main/panel-admin-main
printf '%s\n' \
  'VITE_API_BASE_URL=https://api.qalataldhaman.com/api/v1/' \
  'VITE_FILE_BASE_URL=https://api.qalataldhaman.com/' \
  'VITE_APP_API_VERSION=1' > .env
docker build -f Dockerfile.deploy -t "qalaat-view:`$TAG" .
cd /home
sed -i "s|image: qalaat-view:.*|image: qalaat-view:`$TAG|" docker-compose.yml
docker compose up -d --force-recreate qalaat_view
docker ps --filter name=qalaat --format 'table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}'
"@ -replace "`r`n", "`n"

try {
    $b64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($bash))
    ssh -o StrictHostKeyChecking=yes -o PreferredAuthentications=password -o PubkeyAuthentication=no `
        "root@$HostName" "echo $b64 | base64 -d | bash"
    if ($LASTEXITCODE -ne 0) { throw "deploy failed ($LASTEXITCODE)" }
    Write-Host 'Deploy finished.' -ForegroundColor Green
}
finally {
    Remove-Item $askpass -Force -ErrorAction SilentlyContinue
}
