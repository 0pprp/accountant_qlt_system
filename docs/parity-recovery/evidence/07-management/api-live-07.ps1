$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200'
$log = @()

function Login($user, $pass) {
  $body = @{ username = $user; password = $pass } | ConvertTo-Json
  return (Invoke-RestMethod -Uri "$base/api/v1/app/Auth/login" -Method Post -Body $body -ContentType 'application/json').accessToken
}
function AuthHeaders($t) { @{ Authorization = "Bearer $t" } }

# Director: summary + delegate followup + follower directory
$tokD = Login 'director_iso' 'admin123'
$hD = AuthHeaders $tokD
try {
  $m = Invoke-RestMethod -Uri "$base/api/v1/admin/ManagementSummary/matrix?year=2026&month=11&kind=collections" -Headers $hD
  $log += "SUMMARY_MATRIX branches=$($m.branches.Count) days=$($m.daysInMonth)"
} catch { $log += "SUMMARY_MATRIX_ERR=$($_.Exception.Response.StatusCode.value__)" }

try {
  $lists = Invoke-RestMethod -Uri "$base/api/v1/admin/DelegateFollowup/lists" -Headers $hD
  $count = if ($lists -is [array]) { $lists.Count } elseif ($lists.items) { $lists.items.Count } else { 0 }
  $log += "DELEGATE_LISTS count=$count"
} catch { $log += "DELEGATE_LISTS_ERR=$($_.Exception.Response.StatusCode.value__)" }

try {
  $fw = Invoke-RestMethod -Uri "$base/api/v1/admin/FollowerWork/followers" -Headers $hD
  $fc = if ($fw -is [array]) { $fw.Count } elseif ($fw.items) { $fw.items.Count } else { 0 }
  $log += "FOLLOWER_DIR count=$fc"
} catch { $log += "FOLLOWER_DIR_ERR=$($_.Exception.Response.StatusCode.value__)" }

# Monitor: summary default denied (no ManagementSummary in seed); ratings allowed
$tokM = Login 'monitor_iso' 'admin123'
$hM = AuthHeaders $tokM
try {
  Invoke-RestMethod -Uri "$base/api/v1/admin/ManagementSummary/matrix?year=2026&month=11&kind=collections" -Headers $hM | Out-Null
  $log += 'MONITOR_SUMMARY=UNEXPECTED_OK'
} catch {
  $log += "MONITOR_SUMMARY_STATUS=$($_.Exception.Response.StatusCode.value__)"
}
try {
  Invoke-RestMethod -Uri "$base/api/v1/admin/ManagementRatings/branches" -Headers $hM | Out-Null
  $log += 'MONITOR_RATINGS=OK'
} catch { $log += "MONITOR_RATINGS_ERR=$($_.Exception.Response.StatusCode.value__)" }

# Limited account without oversight
try {
  $tokB = Login 'bm_iso' 'admin123'
  $hB = AuthHeaders $tokB
  try {
    Invoke-RestMethod -Uri "$base/api/v1/admin/ManagementSummary/matrix?year=2026&month=11&kind=collections" -Headers $hB | Out-Null
    $log += 'BM_SUMMARY=UNEXPECTED_OK'
  } catch { $log += "BM_SUMMARY_STATUS=$($_.Exception.Response.StatusCode.value__)" }
  try {
    $active = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/active" -Headers $hB
    $log += "BM_ACTIVE_VISIT=$([bool]$active)"
  } catch { $log += "BM_ACTIVE_VISIT_ERR=$($_.Exception.Response.StatusCode.value__)" }
} catch {
  $log += "BM_LOGIN_ERR=$($_.Exception.Message)"
}

$log | ForEach-Object { $_ }
$log | Set-Content 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\07-management\api-live-07.log' -Encoding utf8
