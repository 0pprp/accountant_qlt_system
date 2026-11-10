$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200/api/v1'
$psql = 'C:\Program Files\PostgreSQL\18\bin\psql.exe'
$env:PGPASSWORD = 'postgres'
$outDir = 'C:\GitHub\accountant_qlt_system\docs\sales-request-review\04f-live-evidence'
$log = Join-Path $outDir 'api-live-log2.jsonl'
'' | Set-Content -Path $log -Encoding utf8

function Ev($obj) {
  $line = ($obj | ConvertTo-Json -Compress -Depth 10)
  Add-Content -Path $log -Value $line -Encoding utf8
  Write-Host $line
}
function Snip($s, $n = 600) {
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

# Second Admin for exception decide (self-approval rule)
$crDec = Api POST "$base/admin/Users" $admin @{
  fullName='Decider Admin'; motherName='N/A'; username='admin_decider_04f'; password='AdminDec123!'
  nationalCode='100000099'; birthDate='1988-01-01'; roleId=1; branchIds=@(1)
  address='addr-d'; phoneNumber='07700000099'
}
Ev @{ step = 'create_decider'; ok = $crDec.ok; status = $crDec.status; body = (Snip $crDec.raw 200) }
& $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -f "$outDir\_complete_emps2.sql" | Out-Null

$empAId = 3; $empBId = 4
$reqId = 3

$assign = Api POST "$base/admin/SalesRequests/$reqId/assign" $admin @{ employeeId = $empAId; reason = '04F assign A' }
Ev @{ step = 'assign_to_A'; ok = $assign.ok; status = $assign.status; body = $assign.raw }

$tokA = Login 'app/Auth/login' 'emp_a_04f' 'EmpA123!'
$tokB = Login 'app/Auth/login' 'emp_b_04f' 'EmpB123!'

$listA = Api GET "$base/app/SalesRequests?tab=incoming" $tokA $null
$listB = Api GET "$base/app/SalesRequests?tab=incoming" $tokB $null
Ev @{ step = 'A_incoming'; body = (Snip $listA.raw 400) }
Ev @{ step = 'B_incoming_should_empty'; body = (Snip $listB.raw 400) }

$view = Api POST "$base/app/SalesRequests/$reqId/view" $tokA $null
$prep = Api POST "$base/app/SalesRequests/$reqId/prepare" $tokA @{ note = 'ready' }
Ev @{ step = 'view'; ok = $view.ok; status = $view.status; body = $view.raw }
Ev @{ step = 'prepare'; ok = $prep.ok; status = $prep.status; body = $prep.raw }

$counts1 = Api GET "$base/admin/SalesRequests/counts" $admin $null
$timeline1 = Api GET "$base/admin/SalesRequests/$reqId/timeline" $admin $null
Ev @{ step = 'admin_counts'; body = (Snip $counts1.raw 700) }
Ev @{ step = 'admin_timeline'; body = (Snip $timeline1.raw 700) }

$insp = Api POST "$base/app/SalesRequests/$reqId/inspect" $tokA $null
Ev @{ step = 'inspect'; ok = $insp.ok; status = $insp.status; body = $insp.raw }
$d1 = Api POST "$base/app/SalesRequests/$reqId/ensure-draft" $tokA $null
$d2 = Api POST "$base/app/SalesRequests/$reqId/ensure-draft" $tokA $null
Ev @{ step = 'ensure_draft_1'; ok = $d1.ok; status = $d1.status; body = $d1.raw }
Ev @{ step = 'ensure_draft_2'; ok = $d2.ok; status = $d2.status; body = $d2.raw }

'SELECT "Id", "LinkedOrderId", "Status" FROM "SalesRequests" WHERE "Id"=3; SELECT COUNT(*) FROM "Orders";' |
  Set-Content "$outDir\_linked2.sql" -Encoding ascii
$linkedCheck = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -F '|' -f "$outDir\_linked2.sql"
Ev @{ step = 'linked_order_idempotency'; linkedCheck = @($linkedCheck) }

$re = Api POST "$base/admin/SalesRequests/$reqId/assign" $admin @{ employeeId = $empBId; reason = '04F reassign B' }
Ev @{ step = 'reassign_to_B'; ok = $re.ok; status = $re.status; body = $re.raw }
$prepA2 = Api POST "$base/app/SalesRequests/$reqId/prepare" $tokA @{ note = 'should fail' }
Ev @{ step = 'A_blocked_after_reassign'; ok = $prepA2.ok; status = $prepA2.status; body = $prepA2.raw }
$getA = Api GET "$base/app/SalesRequests/$reqId" $tokA $null
$getB = Api GET "$base/app/SalesRequests/$reqId" $tokB $null
Ev @{ step = 'A_get_after_reassign'; ok = $getA.ok; status = $getA.status; body = (Snip $getA.raw 300) }
Ev @{ step = 'B_get_after_reassign'; ok = $getB.ok; status = $getB.status; body = (Snip $getB.raw 300) }

# Exception decide with second admin
$exId = 4
$tokDec = Login 'admin/Auth/login' 'admin_decider_04f' 'AdminDec123!'
$appr = Api POST "$base/admin/SalesRequests/$exId/approve-exception" $tokDec @{ reason = 'approved by decider' }
Ev @{ step = 'approve_exception_decider'; ok = $appr.ok; status = $appr.status; body = $appr.raw }
$exDetail = Api GET "$base/admin/SalesRequests/$exId" $admin $null
Ev @{ step = 'exception_detail_after_approve'; body = (Snip $exDetail.raw 700) }

# Reject exception path on new request
@"
WITH c AS (SELECT "Id" FROM "Customers" WHERE "PhoneNumber"='07701112233' ORDER BY "Id" DESC LIMIT 1)
INSERT INTO "SalesRequests" (
  "CustomerName","CustomerPhone","SourceType","Status","BranchId",
  "ExistingCustomerId","IsDeleted","ConcurrencyStamp","CreatedBy","CreatedAt"
) SELECT '04F Ex2','07701112233',0,0,1,c."Id",false,0,1,NOW() FROM c
RETURNING "Id";
"@ | Set-Content "$outDir\_seed_ex2.sql" -Encoding utf8
$ex2Out = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -f "$outDir\_seed_ex2.sql"
$ex2Id = (@($ex2Out) | Where-Object { $_ -match '^\d+$' } | Select-Object -Last 1)
[void](Api POST "$base/admin/SalesRequests/$ex2Id/request-exception" $admin @{ reason = 'need reject path' })
$rejEx = Api POST "$base/admin/SalesRequests/$ex2Id/reject-exception" $tokDec @{ reason = 'rejected by decider' }
Ev @{ step = 'reject_exception'; ok = $rejEx.ok; status = $rejEx.status; body = $rejEx.raw; ex2Id = $ex2Id }

# Complete sale
$firstLine = @($linkedCheck) | Select-Object -First 1
$parts = "$firstLine" -split '\|'
$linkedOrderId = if ($parts.Count -ge 2) { $parts[1] } else { '' }
Ev @{ step = 'parse_linked'; firstLine = "$firstLine"; linkedOrderId = $linkedOrderId }
if ($linkedOrderId -and $linkedOrderId -ne '' -and $linkedOrderId -ne '\N') {
  & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -c "UPDATE `"Orders`" SET `"SellerId`"=$empBId WHERE `"Id`"=$linkedOrderId;" | Out-Null
  $complete = Api POST "$base/app/SalesWorkflow/$linkedOrderId/complete" $tokB $null
  Ev @{ step = 'complete_sale'; ok = $complete.ok; status = $complete.status; body = $complete.raw }
  'SELECT "Id", "LinkedOrderId", "Status" FROM "SalesRequests" WHERE "Id"=3;' | Set-Content "$outDir\_after2.sql" -Encoding ascii
  $after = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -F '|' -f "$outDir\_after2.sql"
  Ev @{ step = 'request_after_complete'; after = @($after) }
}

# Outbox rows
$ob = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -F '|' -c 'SELECT "Id","RequestId","ChangeType","PublishedAtUtc","RetryCount" FROM "SalesRequestChangeOutbox" ORDER BY "Id";'
Ev @{ step = 'outbox_rows'; rows = @($ob) }

Ev @{ step = 'DONE2' }
Write-Host 'LIVE_CONTINUE_FINISHED'
