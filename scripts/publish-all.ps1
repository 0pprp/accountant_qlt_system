#Requires -Version 5.1
<#
.SYNOPSIS
  Clean then publish API + admin panel into publish-out/.
#>
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'clean.ps1')
& (Join-Path $PSScriptRoot 'publish-api.ps1')
& (Join-Path $PSScriptRoot 'publish-panel.ps1')
Write-Host "All publish steps finished. Output: publish-out/" -ForegroundColor Green
