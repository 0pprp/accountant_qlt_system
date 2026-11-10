$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200'
$out = @()

function Login($user, $pass) {
  $body = @{ username = $user; password = $pass } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$base/api/v1/app/Auth/login" -Method Post -Body $body -ContentType 'application/json'
  return $r.accessToken
}

function AuthHeaders($token) {
  @{ Authorization = "Bearer $token"; 'Content-Type' = 'application/json' }
}

# Motaba
$token = Login 'follower_iso' 'admin123'
$h = AuthHeaders $token
$out += "LOGIN_MOTABA=OK"

$lists = Invoke-RestMethod -Uri "$base/api/v1/app/committee/lists" -Headers $h
$out += "LISTS_COUNT=$($lists.Count)"

$stats = Invoke-RestMethod -Uri "$base/api/v1/app/committee/list-stats" -Headers $h
$out += "LIST_STATS_ITEMS=$($stats.items.Count); YM=$($stats.yearMonth); PREV_AVAILABLE=$($stats.items[0].previousMonthAvailable)"

$daily = Invoke-RestMethod -Uri "$base/api/v1/app/committee/daily?listId=1&showType=%D8%A7%D9%84%D8%BA%D9%8A%D8%B1%20%D9%85%D8%B3%D8%AF%D8%AF%D9%8A%D9%86" -Headers $h
$out += "DAILY_CUSTOMERS=$($daily.customers.Count); HASNOTE0=$($daily.customers[0].hasNote)"

$custId = $daily.customers[0].customerId
$name = $daily.customers[0].fullName
$phone = $daily.customers[0].phone

$add = Invoke-RestMethod -Uri "$base/api/v1/app/committee/field-followups" -Method Post -Headers $h -Body (@{
  listId = 1; listName = 'قائمة تحقق'; customerId = $custId; customerName = $name; phoneNumber = $phone
} | ConvertTo-Json)
$out += "FIELD_ADD id=$($add.id) status=$($add.status)"

$pending = Invoke-RestMethod -Uri "$base/api/v1/app/committee/field-followups" -Headers $h
$out += "FIELD_PENDING=$($pending.Count)"

$add2 = Invoke-RestMethod -Uri "$base/api/v1/app/committee/field-followups" -Method Post -Headers $h -Body (@{
  listId = 1; listName = 'قائمة تحقق'; customerId = $custId; customerName = $name; phoneNumber = $phone
} | ConvertTo-Json)
$out += "FIELD_ADD_IDEMPOTENT sameId=$([bool]($add2.id -eq $add.id))"

$rc = Invoke-RestMethod -Uri "$base/api/v1/app/committee/field-followups/record-complete" -Method Post -Headers $h -Body (@{
  listId = 1; listName = 'قائمة تحقق'; customerId = $custId; customerName = $name; phoneNumber = $phone; note = 'زيارة ميدانية iso'
} | ConvertTo-Json)
$out += "RECORD_COMPLETE status=$($rc.status) note=$($rc.completionNote)"

$notes = Invoke-RestMethod -Uri "$base/api/v1/app/committee/field-followups/latest-notes?listId=1" -Headers $h
$out += "LATEST_NOTES=$($notes.Count) first=$($notes[0].note)"

# Delegate note
$dn = Invoke-RestMethod -Uri "$base/api/v1/app/committee/order-lists/1/delegate-notes" -Method Post -Headers $h -Body (@{
  noteText = 'ملاحظة مندوب iso 05'
} | ConvertTo-Json)
$out += "DELEGATE_NOTE id=$($dn.id)"

# Customer note
$cn = Invoke-RestMethod -Uri "$base/api/v1/app/committee/customers/$custId/notes" -Method Post -Headers $h -Body (@{
  listId = 1; noteText = 'ملاحظة زبون iso 05'
} | ConvertTo-Json)
$out += "CUSTOMER_NOTE id=$($cn.id)"

$daily2 = Invoke-RestMethod -Uri "$base/api/v1/app/committee/daily?listId=1&showType=%D8%A7%D9%84%D8%BA%D9%8A%D8%B1%20%D9%85%D8%B3%D8%AF%D8%AF%D9%8A%D9%86" -Headers $h
$matched = $daily2.customers | Where-Object { $_.customerId -eq $custId } | Select-Object -First 1
$out += "HASNOTE_AFTER=$($matched.hasNote); LATEST_FIELD=$($matched.latestFieldFollowupNote)"

$out | ForEach-Object { $_ }
$out | Set-Content -Path 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\05-committee\api-live-05.log' -Encoding utf8
