#Requires -Version 5.1
<#
.SYNOPSIS
  Pushes the current branch to origin (GitHub).
  Optional: -Message "commit msg" to commit staged+related changes first.
#>
param(
    [string]$Message = '',
    [switch]$SkipCommit
)

$ErrorActionPreference = 'Stop'
$Root = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $Root

$branch = (git rev-parse --abbrev-ref HEAD).Trim()
if (-not $branch) { throw 'Not inside a git repository' }

Write-Host "==> Branch: $branch" -ForegroundColor Cyan
git status -sb

if (-not $SkipCommit -and $Message) {
    git add -A
    $staged = git diff --cached --name-only
    if (-not $staged) {
        Write-Host "Nothing to commit." -ForegroundColor Yellow
    } else {
        git commit -m $Message
        if ($LASTEXITCODE -ne 0) { throw "git commit failed ($LASTEXITCODE)" }
    }
}

Write-Host "==> Pushing to origin/$branch" -ForegroundColor Cyan
git push -u origin HEAD
if ($LASTEXITCODE -ne 0) { throw "git push failed ($LASTEXITCODE)" }
Write-Host "Pushed to GitHub." -ForegroundColor Green
