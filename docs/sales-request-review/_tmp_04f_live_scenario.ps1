# 04F live scenario against isolated API + qalaat_04f_sales_verify
# Documented SalesRequest creation = direct SQL insert (NOT ingestion proof).
$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200/api/v1'
$outDir = 'C:\GitHub\accountant_qlt_system\docs\sales-request-review\04f-live-evidence'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$log = Join-Path $outDir 'api-live-log.jsonl'

function Write-Evidence($obj) {
  ($obj | ConvertTo-Json -Compress -Depth 8) | Add-Content -Path $log -Encoding utf8
  Write-Output ($obj | ConvertTo-Json -Compress -Depth 6)
}

function Invoke-Api {
  param($Method, $Url, $Token, $Body)
  $headers = @{ 'Content-Type' = 'application/json' }
  if ($Token) { $headers['Authorization'] = "Bearer $Token" }
  $json = if ($null -ne $Body) { $Body | ConvertTo-Json -Depth 8 } else { $null }
  try {
    if ($json) {
      $r = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers -Body $json -UseBasicParsing
    } else {
      $r = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers -UseBasicParsing
    }
    return @{ ok = $true; status = [int]$r.StatusCode; body = $r.Content }
  } catch {
    $resp = $_.Exception.Response
    $code = if ($resp) { [int]$resp.StatusCode } else { 0 }
    $reader = if ($resp) { [System.IO.StreamReader]::new($resp.GetResponseStream()) } else { $null }
    $text = if ($reader) { $reader.ReadToEnd() } else { $_.Exception.Message }
    return @{ ok = $false; status = $code; body = $text }
  }
}

function Login-Admin {
  $r = Invoke-Api POST "$base/admin/Auth/login" $null @{ username = 'admin'; password = 'admin123' }
  Write-Evidence @{ step = 'admin_login'; result = $r }
  if (-not $r.ok) { throw "admin login failed: $($r.body)" }
  $parsed = $r.body | ConvertFrom-Json
  # tolerate wrapped or flat token shapes
  $token = $parsed.data.accessToken
  if (-not $token) { $token = $parsed.accessToken }
  if (-not $token) { $token = $parsed.data.token }
  if (-not $token) { throw "no access token in $($r.body)" }
  return $token
}

$adminToken = Login-Admin

# Create Mandob employees A/B if missing
$createA = @{
  fullName = 'Employee A'; motherName = 'N/A'; username = 'emp_a_04f'; password = 'EmpA123!'
  nationalCode = '100000001'; birthDate = '1990-01-01'; roleId = 7; branchIds = @(1)
  address = 'addr-a'; phoneNumber = '07700000001'
}
$createB = @{
  fullName = 'Employee B'; motherName = 'N/A'; username = 'emp_b_04f'; password = 'EmpB123!'
  nationalCode = '100000002'; birthDate = '1990-01-01'; roleId = 7; branchIds = @(1)
  address = 'addr-b'; phoneNumber = '07700000002'
}

$rA = Invoke-Api POST "$base/admin/Users" $adminToken $createA
Write-Evidence @{ step = 'create_emp_a'; result = $rA }
$rB = Invoke-Api POST "$base/admin/Users" $adminToken $createB
Write-Evidence @{ step = 'create_emp_b'; result = $rB }

# Resolve employee ids via assignable employees or users list
$users = Invoke-Api GET "$base/admin/Users?pageNumber=1&pageSize=50" $adminToken $null
Write-Evidence @{ step = 'list_users'; result = $users }
$usersParsed = $users.body | ConvertFrom-Json
# try several shapes
$items = $usersParsed.data.items
if (-not $items) { $items = $usersParsed.items }
if (-not $items) { $items = $usersParsed.data }
$empA = $items | Where-Object { $_.username -eq 'emp_a_04f' } | Select-Object -First 1
$empB = $items | Where-Object { $_.username -eq 'emp_b_04f' } | Select-Object -First 1
if (-not $empA -or -not $empB) {
  # fallback SQL ids will be filled by companion sql
  Write-Evidence @{ step = 'users_not_found_in_list'; a = $empA; b = $empB; raw = $users.body.Substring(0, [Math]::Min(500, $users.body.Length)) }
}

Write-Output "SCRIPT_PARTIAL_DONE admin=$($adminToken.Substring(0,12))..."
