$ErrorActionPreference = 'Stop'
$baseA = 'http://127.0.0.1:4200'
$baseB = 'http://127.0.0.1:4201'
$log = @()

function Login($base, $user, $pass) {
  $body = @{ username = $user; password = $pass } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$base/api/v1/app/Auth/login" -Method Post -Body $body -ContentType 'application/json'
  return $r.accessToken
}
function AuthHeaders($token) { @{ Authorization = "Bearer $token"; 'Content-Type' = 'application/json' } }

# health
foreach ($b in @($baseA, $baseB)) {
  try {
    Invoke-RestMethod -Uri "$b/api/v1/app/Auth/login" -Method Post -Body (@{ username='mandob_iso'; password='admin123' } | ConvertTo-Json) -ContentType 'application/json' | Out-Null
    $log += "UP $b"
  } catch {
    throw "API not up: $b — $($_.Exception.Message)"
  }
}

py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\toggle-collection.py' 0 | Out-Null
$second = (py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\ensure-second-order.py' | Select-Object -Last 1).ToString().Trim()
$log += "SECOND_ORDER $second"
$order2 = [int]($second.Split('|')[1])
$sellerId = [int]($second.Split('|')[2])

py -3 -c @"
import psycopg2
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('UPDATE \"Orders\" SET \"ExecutionStatus\"=1, \"SellAmount\"=500000 WHERE \"Id\"=1')
cur.execute('UPDATE \"Orders\" SET \"ExecutionStatus\"=1, \"SellAmount\"=500000 WHERE \"Id\"=%s', ($order2,))
c.commit(); print('ORDERS_ROOMY')
c.close()
"@ | ForEach-Object { $log += $_ }

$tok = Login $baseA 'mandob_iso' 'admin123'
$h = AuthHeaders $tok
$date = (Get-Date).ToString('yyyy-MM-dd')

# 1) Create on API-A, Sync on API-B — same key/order/amount/date
$key1 = [guid]::NewGuid().ToString()
$create = Invoke-RestMethod -Uri "$baseA/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body (@{ amount=1000; clientPaymentId=$key1; date=$date } | ConvertTo-Json)
$sync = Invoke-RestMethod -Uri "$baseB/api/v1/app/InstallmentPayments/sync" -Method Post -Headers $h -Body (@{ installmentPayments=@(@{ orderId=1; amount=1000; date=$date; clientPaymentId=$key1 }) } | ConvertTo-Json)
$log += "CROSS_CREATE_SYNC createId=$($create.paymentId) ack=$($sync.items[0].alreadyApplied) sameId=$([bool]($create.paymentId -eq $sync.items[0].paymentId)) orderId=$($sync.items[0].orderId)"
$log += (py -3 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\count-key.py' $key1)

# 2) Two keys exceed remaining — Create on A and Create on B concurrently
py -3 -c @"
import psycopg2
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('SELECT COALESCE(SUM(\"Amount\"),0) FROM \"InstallmentPayments\" WHERE \"OrderId\"=1')
paid=float(cur.fetchone()[0])
cur.execute('UPDATE \"Orders\" SET \"SellAmount\"=%s, \"ExecutionStatus\"=1 WHERE \"Id\"=1', (paid+1500,))
c.commit(); print('SELL_TIGHT', paid+1500)
c.close()
"@ | ForEach-Object { $log += $_ }

$kA = [guid]::NewGuid().ToString(); $kB = [guid]::NewGuid().ToString()
$j1 = Start-Job -ScriptBlock {
  param($base,$h,$body)
  try { $r=Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body; "A_OK:$($r.paymentId)" }
  catch { "A_ERR:$($_.Exception.Response.StatusCode.value__)" }
} -ArgumentList $baseA,$h,(@{ amount=1000; clientPaymentId=$kA; date=$date } | ConvertTo-Json)
$j2 = Start-Job -ScriptBlock {
  param($base,$h,$body)
  try { $r=Invoke-RestMethod -Uri "$base/api/v1/app/Orders/1/installment-payments" -Method Post -Headers $h -Body $body; "B_OK:$($r.paymentId)" }
  catch { "B_ERR:$($_.Exception.Response.StatusCode.value__)" }
} -ArgumentList $baseB,$h,(@{ amount=1000; clientPaymentId=$kB; date=$date } | ConvertTo-Json)
$j1,$j2 | Wait-Job | Out-Null
$over = @((Receive-Job $j1), (Receive-Job $j2)); Remove-Job $j1,$j2
$log += ($over -join ' | ')
$okN = @($over | Where-Object { $_ -match '_OK:' }).Count
$errN = @($over | Where-Object { $_ -match '_ERR:' }).Count
$log += "OVER_DUAL ok=$okN err=$errN expectOneEach=$([bool]($okN -eq 1 -and $errN -eq 1))"

# 3) Two different orders, same cash holder — concurrent Create A/B; cash delta = 2000
py -3 -c @"
import psycopg2
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('UPDATE \"Orders\" SET \"SellAmount\"=500000, \"ExecutionStatus\"=1 WHERE \"Id\" IN (1, %s)' % ($order2,))
c.commit()
c.close()
"@ | Out-Null

$cashBefore = py -3 -c @"
import psycopg2,sys
sid=int(sys.argv[1])
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('SELECT \"UndeliveredCashAmount\" FROM \"Users\" WHERE \"Id\"=%s', (sid,))
print(cur.fetchone()[0])
c.close()
"@ $sellerId
$cash0 = [double](($cashBefore | Select-Object -Last 1).ToString())
$log += "CASH_BEFORE seller=$sellerId cash=$cash0"

$kO1 = [guid]::NewGuid().ToString(); $kO2 = [guid]::NewGuid().ToString()
$c1 = Start-Job -ScriptBlock {
  param($base,$h,$oid,$body)
  try { $r=Invoke-RestMethod -Uri "$base/api/v1/app/Orders/$oid/installment-payments" -Method Post -Headers $h -Body $body; "O1_OK:$($r.paymentId)" }
  catch { "O1_ERR:$($_.Exception.Message)" }
} -ArgumentList $baseA,$h,1,(@{ amount=1000; clientPaymentId=$kO1; date=$date } | ConvertTo-Json)
$c2 = Start-Job -ScriptBlock {
  param($base,$h,$oid,$body)
  try { $r=Invoke-RestMethod -Uri "$base/api/v1/app/Orders/$oid/installment-payments" -Method Post -Headers $h -Body $body; "O2_OK:$($r.paymentId)" }
  catch { "O2_ERR:$($_.Exception.Message)" }
} -ArgumentList $baseB,$h,$order2,(@{ amount=1000; clientPaymentId=$kO2; date=$date } | ConvertTo-Json)
$c1,$c2 | Wait-Job | Out-Null
$cashJobs = @((Receive-Job $c1), (Receive-Job $c2)); Remove-Job $c1,$c2
$log += ($cashJobs -join ' | ')
$cashAfter = py -3 -c @"
import psycopg2,sys
sid=int(sys.argv[1])
c=psycopg2.connect(host='localhost',port=5432,dbname='qalaat_22_23_iso',user='postgres',password='postgres')
cur=c.cursor()
cur.execute('SELECT \"UndeliveredCashAmount\" FROM \"Users\" WHERE \"Id\"=%s', (sid,))
print(cur.fetchone()[0])
c.close()
"@ $sellerId
$cash1 = [double](($cashAfter | Select-Object -Last 1).ToString())
$delta = $cash1 - $cash0
$bothOk = ($cashJobs | Where-Object { $_ -match '_OK:' }).Count -eq 2
$log += "DUAL_ORDERS_SAME_CASH after=$cash1 delta=$delta bothOk=$bothOk noLostUpdate=$([bool]($bothOk -and $delta -eq 2000))"

$log | ForEach-Object { $_ }
$log | Set-Content 'C:\GitHub\accountant_qlt_system\docs\parity-recovery\evidence\06-delegate\api-live-dual-process-locks.log' -Encoding utf8
