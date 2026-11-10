$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200'
$log = @()

function Login($user, $pass) {
  $body = @{ username = $user; password = $pass } | ConvertTo-Json
  return (Invoke-RestMethod -Uri "$base/api/v1/app/Auth/login" -Method Post -Body $body -ContentType 'application/json').accessToken
}
function AuthH($t) { @{ Authorization = "Bearer $t"; 'Content-Type' = 'application/json' } }

# Prefer delegated manager / director with BranchVisit.Open
$user = 'director_iso'
try { $tok = Login $user 'admin123' } catch { $user='mandob_iso'; $tok = Login $user 'admin123' }
$h = AuthH $tok
$log += "LOGIN_AS=$user"

# Resolve a branch in scope
$profile = Invoke-RestMethod -Uri "$base/api/v1/app/Users/profile" -Headers $h
$branchId = $null
if ($profile.branches -and $profile.branches.Count -gt 0) { $branchId = $profile.branches[0].id }
elseif ($profile.branchIds -and $profile.branchIds.Count -gt 0) { $branchId = $profile.branchIds[0] }
else {
  # admin branches list fallback
  try {
    $br = Invoke-RestMethod -Uri "$base/api/v1/admin/Branches?page=1&pageSize=5" -Headers $h
    $branchId = $br.items[0].id
  } catch {}
}
$log += "BRANCH_ID=$branchId"

if (-not $branchId) { throw 'no branch' }

# Open visit idempotent
$v1 = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/open" -Method Post -Headers $h -Body (@{ branchId = $branchId } | ConvertTo-Json)
$log += "OPEN1 id=$($v1.id) status=$($v1.status) branch=$($v1.branchId)"
$v2 = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/open" -Method Post -Headers $h -Body (@{ branchId = $branchId } | ConvertTo-Json)
$log += "OPEN2_IDEMPOTENT sameId=$([bool]($v1.id -eq $v2.id))"

# Other branch while active → reject
$other = $branchId + 1
try {
  Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/open" -Method Post -Headers $h -Body (@{ branchId = $other } | ConvertTo-Json) | Out-Null
  $log += 'OTHER_BRANCH=UNEXPECTED_OK'
} catch {
  $log += "OTHER_BRANCH_STATUS=$($_.Exception.Response.StatusCode.value__)"
}

$active = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/active" -Headers $h
$log += "ACTIVE id=$($active.id) branch=$($active.branchId)"

# WrongSale / NewSales / Tasks smoke (permission gated)
foreach ($path in @(
  '/api/v1/admin/WrongSales?audience=delegate&page=1&pageSize=5',
  '/api/v1/admin/BranchNewSales?page=1&pageSize=5',
  '/api/v1/admin/ManagementTasks?page=1&pageSize=5&inboxForMe=true'
)) {
  try {
    Invoke-RestMethod -Uri "$base$path" -Headers $h | Out-Null
    $log += "OK $path"
  } catch {
    $log += "STATUS=$($_.Exception.Response.StatusCode.value__) $path"
  }
}

$log | ForEach-Object { $_ }
$log | Set-Content 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\08-branch-delegate\api-live-08.log' -Encoding utf8
