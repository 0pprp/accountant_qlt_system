$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200/api/v1'
$psql = 'C:\Program Files\PostgreSQL\18\bin\psql.exe'
$env:PGPASSWORD = 'postgres'
$outDir = 'C:\GitHub\accountant_qlt_system\docs\sales-request-review\04f-live-evidence'
$log = Join-Path $outDir 'api-live-log3.jsonl'
'' | Set-Content -Path $log -Encoding utf8

function Ev($obj) {
  $line = ($obj | ConvertTo-Json -Compress -Depth 10)
  Add-Content -Path $log -Value $line -Encoding utf8
  Write-Host $line
}
function Snip($s, $n = 700) {
  if ($null -eq $s) { return '' }
  return $s.Substring(0, [Math]::Min($n, $s.Length))
}
function Api($Method, $Url, $Token, $Body) {
  $headers = @{ 'Content-Type' = 'application/json' }
  if ($Token) { $headers['Authorization'] = "Bearer $Token" }
  $json = if ($null -ne $Body) { ($Body | ConvertTo-Json -Depth 8 -Compress) } else { $null }
  try {
    if ($null -ne $json) {
      $r = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($json)) -UseBasicParsing
    } else {
      $r = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers -UseBasicParsing
    }
    $parsed = $null; try { $parsed = $r.Content | ConvertFrom-Json } catch {}
    return @{ ok = $true; status = [int]$r.StatusCode; raw = $r.Content; json = $parsed }
  } catch {
    $resp = $_.Exception.Response; $code = 0; $text = $_.Exception.Message
    if ($resp) {
      $code = [int]$resp.StatusCode
      $stream = $resp.GetResponseStream()
      if ($stream) { $text = (New-Object System.IO.StreamReader($stream)).ReadToEnd() }
    }
    return @{ ok = $false; status = $code; raw = $text; json = $null }
  }
}
function Login($path, $user, $pass) {
  $r = Api POST "$base/$path" $null @{ username = $user; password = $pass }
  Ev @{ step = "login_$user"; ok = $r.ok; status = $r.status }
  if (-not $r.ok) { throw "login failed $user $($r.raw)" }
  return $r.json.accessToken
}

$admin = Login 'admin/Auth/login' 'admin' 'admin123'
$tokA = Login 'app/Auth/login' 'emp_a_04f' 'EmpA123!'
$tokB = Login 'app/Auth/login' 'emp_b_04f' 'EmpB123!'

$cr = Api POST "$base/admin/Users" $admin @{
  fullName='Exception Decider'; motherName='N/A'; username='ex_decider_04f'; password='ExDec123!'
  nationalCode='100000088'; birthDate='1985-05-05'; roleId=8; branchIds=@(1)
  address='addr-x'; phoneNumber='07700000088'
}
Ev @{ step = 'create_ex_decider'; ok = $cr.ok; status = $cr.status; body = (Snip $cr.raw 200) }

$g = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -f "$outDir\_grant_decider.sql"
Ev @{ step = 'grant_decider'; g = @($g) }

$tokDec = Login 'admin/Auth/login' 'ex_decider_04f' 'ExDec123!'

$appr = Api POST "$base/admin/SalesRequests/4/approve-exception" $tokDec @{ reason = 'approved by decider' }
Ev @{ step = 'approve_exception'; ok = $appr.ok; status = $appr.status; body = $appr.raw }
$exDetail = Api GET "$base/admin/SalesRequests/4" $admin $null
Ev @{ step = 'exception_detail'; body = (Snip $exDetail.raw) }

$ex2Out = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -f "$outDir\_seed_ex2.sql"
$ex2Id = (@($ex2Out) | Where-Object { $_ -match '^\d+$' } | Select-Object -Last 1)
[void](Api POST "$base/admin/SalesRequests/$ex2Id/request-exception" $admin @{ reason = 'need reject path' })
$rej = Api POST "$base/admin/SalesRequests/$ex2Id/reject-exception" $tokDec @{ reason = 'rejected by decider' }
Ev @{ step = 'reject_exception'; ok = $rej.ok; status = $rej.status; body = $rej.raw; ex2Id = $ex2Id }

& $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -f "$outDir\_set_seller.sql" | Out-Null
$complete = Api POST "$base/app/SalesWorkflow/4/complete" $tokB $null
Ev @{ step = 'complete_sale'; ok = $complete.ok; status = $complete.status; body = $complete.raw }
'SELECT "Id", "LinkedOrderId", "Status" FROM "SalesRequests" WHERE "Id"=3;' | Set-Content "$outDir\_after3.sql" -Encoding ascii
$after = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -F '|' -f "$outDir\_after3.sql"
Ev @{ step = 'request_after_complete'; after = @($after) }

$obSql = @'
SELECT "Id","RequestId","ChangeType",
  CASE WHEN "PublishedAtUtc" IS NULL THEN 'null' ELSE 'pub' END AS pub,
  "RetryCount"
FROM "SalesRequestChangeOutbox" ORDER BY "Id";
'@
$obSql | Set-Content "$outDir\_outbox.sql" -Encoding ascii
$ob = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -F '|' -f "$outDir\_outbox.sql"
Ev @{ step = 'outbox_rows'; rows = @($ob) }

& $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -f "$outDir\_deny_a.sql" | Out-Null
$listOld = Api GET "$base/app/SalesRequests?tab=incoming" $tokA $null
$tokA2 = Login 'app/Auth/login' 'emp_a_04f' 'EmpA123!'
$listNew = Api GET "$base/app/SalesRequests?tab=incoming" $tokA2 $null
Ev @{ step = 'deny_AppRead_old_token'; ok = $listOld.ok; status = $listOld.status; body = (Snip $listOld.raw 250) }
Ev @{ step = 'deny_AppRead_new_token'; ok = $listNew.ok; status = $listNew.status; body = (Snip $listNew.raw 250) }

Ev @{ step = 'DONE3' }
Write-Host 'LIVE_FINISH_DONE'
