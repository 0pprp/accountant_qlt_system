#Requires -Version 5.1
<#
.SYNOPSIS
  Cleans build/publish caches so the repo stays light and easy to reset.
#>
$ErrorActionPreference = 'Stop'
$Root = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host "==> Cleaning publish-out" -ForegroundColor Cyan
$publishOut = Join-Path $Root 'publish-out'
if (Test-Path $publishOut) {
    Remove-Item -Recurse -Force $publishOut
}

$patterns = @(
    'bin', 'obj', 'dist', 'build', '.dart_tool', 'coverage', 'node_modules'
)

Write-Host "==> Removing build folders: $($patterns -join ', ')" -ForegroundColor Cyan
Get-ChildItem -Path $Root -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object {
        $patterns -contains $_.Name -and
        $_.FullName -notmatch '\\.git\\' -and
        $_.FullName -notmatch '\\_archive\\'
    } |
    ForEach-Object {
        Write-Host "  remove $($_.FullName.Substring($Root.Path.Length + 1))"
        Remove-Item -Recurse -Force $_.FullName -ErrorAction SilentlyContinue
    }

Write-Host "Clean done." -ForegroundColor Green
