$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200'
$out = @()

function Login($user, $pass) {
  $body = @{ username = $user; password = $pass } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$base/api/v1/app/Auth/login" -Method Post -Body $body -ContentType 'application/json'
  return $r.accessToken
}

$token = Login 'mandob_iso' 'admin123'
$h = @{ Authorization = "Bearer $token"; 'Content-Type' = 'application/json' }
$out += 'LOGIN_MANDOB=OK'

$profile = Invoke-RestMethod -Uri "$base/api/v1/app/Users/profile" -Headers $h
$out += "PROFILE_LIST=$($profile.orderList.id); COLL_ENABLED=$($profile.orderList.followerCollectionEnabled)"

$key = [guid]::NewGuid().ToString()
$body1 = @{ amount = 1000; clientPaymentId = $key } | ConvertTo-Json
try {
  Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body1 | Out-Null
  $out += 'PAY1=OK'
} catch {
  $out += "PAY1_ERR=$($_.Exception.Message)"
}

try {
  Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body1 | Out-Null
  $out += 'PAY1_RETRY_SAME_KEY=OK_IDEMPOTENT'
} catch {
  $out += "PAY1_RETRY_ERR=$($_.Exception.Message)"
}

$out += (py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\toggle-collection.py' 1)
try {
  Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body (@{ amount = 1000; clientPaymentId = [guid]::NewGuid().ToString() } | ConvertTo-Json) | Out-Null
  $out += 'PAY_WHILE_LOCKED=UNEXPECTED_OK'
} catch {
  $resp = $_.ErrorDetails.Message
  if (-not $resp) { $resp = $_.Exception.Message }
  $out += "PAY_WHILE_LOCKED=$resp"
}

$out += (py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\toggle-collection.py' 0)
$out += (py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\count-key.py' $key)

$out | ForEach-Object { $_ }
$out | Set-Content -Path 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\api-live-06.log' -Encoding utf8
