$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200'
$log = @()

function Login($user, $pass) {
  $body = @{ username = $user; password = $pass } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$base/api/v1/app/Auth/login" -Method Post -Body $body -ContentType 'application/json'
  return $r.accessToken
}

function AuthHeaders($token) { @{ Authorization = "Bearer $token"; 'Content-Type' = 'application/json' } }

# Ensure follower collection off + order in progress with room
py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\toggle-collection.py' 0 | Out-Null
py -3 -c @"
import psycopg2
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('UPDATE \"Orders\" SET \"ExecutionStatus\"=1, \"SellAmount\"=500000 WHERE \"Id\"=1')
c.commit(); print('ORDER_RESET')
c.close()
"@ | ForEach-Object { $log += $_ }

$tok = Login 'mandob_iso' 'admin123'
$h = AuthHeaders $tok
$date = (Get-Date).ToString('yyyy-MM-dd')

# 1) Same key concurrent Create+Sync
$key1 = [guid]::NewGuid().ToString()
$bodyCreate = @{ amount = 1000; clientPaymentId = $key1; date = $date } | ConvertTo-Json
$bodySync = @{ installmentPayments = @(@{ orderId = 1; amount = 1000; date = $date; clientPaymentId = $key1 }) } | ConvertTo-Json
$jobs = @()
$jobs += Start-Job -ScriptBlock {
  param($base,$h,$body)
  try { Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body; 'CREATE_OK' }
  catch { "CREATE_ERR:$($_.Exception.Message)" }
} -ArgumentList $base,$h,$bodyCreate
$jobs += Start-Job -ScriptBlock {
  param($base,$h,$body)
  try { Invoke-RestMethod -Uri "$base/api/v1/app/InstallmentPayments/sync" -Method Post -Headers $h -Body $body; 'SYNC_OK' }
  catch { "SYNC_ERR:$($_.Exception.Message)" }
} -ArgumentList $base,$h,$bodySync
$jobs | Wait-Job | Out-Null
$jobs | ForEach-Object { $log += (Receive-Job $_); Remove-Job $_ }
$log += (py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\count-key.py' $key1)

# 2) Same key different amount → 409
try {
  Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body (@{ amount = 2000; clientPaymentId = $key1; date = $date } | ConvertTo-Json) | Out-Null
  $log += 'CONFLICT_AMOUNT=UNEXPECTED_OK'
} catch {
  $code = $_.Exception.Response.StatusCode.value__
  $log += "CONFLICT_AMOUNT_STATUS=$code"
}

# 3) Other account same key → not success / no payload leak
$tok2 = Login 'follower_iso' 'admin123'
$h2 = AuthHeaders $tok2
try {
  Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h2 -Body (@{ amount = 1000; clientPaymentId = $key1; date = $date } | ConvertTo-Json) | Out-Null
  $log += 'OTHER_ACCOUNT=UNEXPECTED_OK'
} catch {
  $resp = $_.Exception.Response
  $code = if ($resp) { [int]$resp.StatusCode } else { -1 }
  $bodyTxt = ''
  if ($resp -and $resp.GetResponseStream) {
    try {
      $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
      $bodyTxt = $sr.ReadToEnd()
      $sr.Close()
    } catch {}
  }
  $hasPaymentId = $bodyTxt -match '"paymentId"\s*:\s*[1-9]'
  $log += "OTHER_ACCOUNT_STATUS=$code leakPaymentId=$hasPaymentId"
}

# 4) Complete sale then retry same key ack; new key rejected
$keyLast = [guid]::NewGuid().ToString()
# shrink remaining so next payment completes
py -3 -c @"
import psycopg2
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('SELECT COALESCE(SUM(\"Amount\"),0) FROM \"InstallmentPayments\" WHERE \"OrderId\"=1')
paid=float(cur.fetchone()[0])
cur.execute('UPDATE \"Orders\" SET \"SellAmount\"=%s, \"ExecutionStatus\"=1 WHERE \"Id\"=1', (paid+1000,))
c.commit(); print('SELL_SET', paid+1000)
c.close()
"@ | ForEach-Object { $log += $_ }

$complete = Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body (@{ amount = 1000; clientPaymentId = $keyLast; date = $date } | ConvertTo-Json)
$log += "COMPLETE_PAY already=$($complete.alreadyApplied) statusOrder=done"
$ack = Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body (@{ amount = 1000; clientPaymentId = $keyLast; date = $date } | ConvertTo-Json)
$log += "ACK_AFTER_COMPLETE already=$($ack.alreadyApplied) paymentId=$($ack.paymentId)"
try {
  Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body (@{ amount = 1000; clientPaymentId = [guid]::NewGuid().ToString(); date = $date } | ConvertTo-Json) | Out-Null
  $log += 'NEW_AFTER_COMPLETE=UNEXPECTED_OK'
} catch {
  $log += "NEW_AFTER_COMPLETE_STATUS=$($_.Exception.Response.StatusCode.value__)"
}

# 5a) Two keys whose sum exceeds remaining → one success, one reject; remaining intact
py -3 -c @"
import psycopg2
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('SELECT COALESCE(SUM(\"Amount\"),0) FROM \"InstallmentPayments\" WHERE \"OrderId\"=1')
paid=float(cur.fetchone()[0])
# remaining room = 1500 so 1000+1000 cannot both apply
cur.execute('UPDATE \"Orders\" SET \"SellAmount\"=%s, \"ExecutionStatus\"=1 WHERE \"Id\"=1', (paid+1500,))
c.commit(); print('SELL_TIGHT', paid+1500)
c.close()
"@ | ForEach-Object { $log += $_ }
$kOverA = [guid]::NewGuid().ToString(); $kOverB = [guid]::NewGuid().ToString()
$overJobs = @()
$overJobs += Start-Job -ScriptBlock {
  param($base,$h,$body)
  try { $r = Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body; "OVER_A_OK:$($r.paymentId)" }
  catch { "OVER_A_ERR:$($_.Exception.Response.StatusCode.value__)" }
} -ArgumentList $base,$h,(@{ amount = 1000; clientPaymentId = $kOverA; date = $date } | ConvertTo-Json)
$overJobs += Start-Job -ScriptBlock {
  param($base,$h,$body)
  try { $r = Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body; "OVER_B_OK:$($r.paymentId)" }
  catch { "OVER_B_ERR:$($_.Exception.Response.StatusCode.value__)" }
} -ArgumentList $base,$h,(@{ amount = 1000; clientPaymentId = $kOverB; date = $date } | ConvertTo-Json)
$overJobs | Wait-Job | Out-Null
$overResults = @($overJobs | ForEach-Object { Receive-Job $_; Remove-Job $_ })
$log += ($overResults -join ' | ')
$overOk = @($overResults | Where-Object { $_ -match '_OK:' }).Count
$overErr = @($overResults | Where-Object { $_ -match '_ERR:' }).Count
$log += "OVER_REMAINING okCount=$overOk errCount=$overErr expectOneEach=$([bool]($overOk -eq 1 -and $overErr -eq 1))"

# 5b) Two allowed concurrent payments same cash holder — no lost cash update
$cashBefore = py -3 -c @"
import psycopg2
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('SELECT COALESCE(SUM(\"Amount\"),0) FROM \"InstallmentPayments\" WHERE \"OrderId\"=1')
paid=float(cur.fetchone()[0])
cur.execute('UPDATE \"Orders\" SET \"SellAmount\"=500000, \"ExecutionStatus\"=1 WHERE \"Id\"=1')
cur.execute('SELECT o.\"SellerId\", u.\"UndeliveredCashAmount\" FROM \"Orders\" o JOIN \"Users\" u ON u.\"Id\"=o.\"SellerId\" WHERE o.\"Id\"=1')
sid, cash = cur.fetchone()
c.commit(); print(f'{sid}|{cash}|{paid}')
c.close()
"@
$parts = ($cashBefore | Select-Object -Last 1).ToString().Split('|')
$sellerId = $parts[0]; $cash0 = [double]$parts[1]
$log += "ORDER_ROOMY seller=$sellerId cashBefore=$cash0"
$kA = [guid]::NewGuid().ToString(); $kB = [guid]::NewGuid().ToString()
$cashJobs = @()
$cashJobs += Start-Job -ScriptBlock {
  param($base,$h,$body)
  try { $r = Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body; "CASH_A:$($r.paymentId)" }
  catch { "CASH_A_ERR:$($_.Exception.Message)" }
} -ArgumentList $base,$h,(@{ amount = 1000; clientPaymentId = $kA; date = $date } | ConvertTo-Json)
$cashJobs += Start-Job -ScriptBlock {
  param($base,$h,$body)
  try { $r = Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body; "CASH_B:$($r.paymentId)" }
  catch { "CASH_B_ERR:$($_.Exception.Message)" }
} -ArgumentList $base,$h,(@{ amount = 1000; clientPaymentId = $kB; date = $date } | ConvertTo-Json)
$cashJobs | Wait-Job | Out-Null
$cashResults = @($cashJobs | ForEach-Object { Receive-Job $_; Remove-Job $_ })
$log += ($cashResults -join ' | ')
$cashAfter = py -3 -c @"
import psycopg2,sys
sid=int(sys.argv[1])
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('SELECT \"UndeliveredCashAmount\" FROM \"Users\" WHERE \"Id\"=%s', (sid,))
print(cur.fetchone()[0])
c.close()
"@ $sellerId
$cash1 = [double]($cashAfter | Select-Object -Last 1)
$delta = $cash1 - $cash0
$log += "TWO_CONCURRENT_CASH after=$cash1 delta=$delta ok=$([bool]($delta -eq 2000))"

# 6) timeout-style retry: same key twice via Create then Sync with fixed order/date
$keyT = [guid]::NewGuid().ToString()
$r1 = Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body (@{ amount = 1000; clientPaymentId = $keyT; date = $date } | ConvertTo-Json)
$r2 = Invoke-RestMethod -Uri "$base/api/v1/app/InstallmentPayments/sync" -Method Post -Headers $h -Body (@{ installmentPayments = @(@{ orderId = 1; amount = 1000; date = $date; clientPaymentId = $keyT }) } | ConvertTo-Json)
$log += "RETRY_CREATE_THEN_SYNC first=$($r1.paymentId) ack=$($r2.items[0].alreadyApplied) sameId=$([bool]($r1.paymentId -eq $r2.items[0].paymentId)) orderId=$($r2.items[0].orderId)"
$log += (py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\count-key.py' $keyT)

$log | ForEach-Object { $_ }
$log | Set-Content 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\api-live-payment-safety.log' -Encoding utf8
