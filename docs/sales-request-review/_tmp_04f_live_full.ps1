$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200/api/v1'
$psql = 'C:\Program Files\PostgreSQL\18\bin\psql.exe'
$env:PGPASSWORD = 'postgres'
$outDir = 'C:\GitHub\accountant_qlt_system\docs\sales-request-review\04f-live-evidence'
$log = Join-Path $outDir 'api-live-log.jsonl'
'' | Set-Content -Path $log -Encoding utf8

function Ev($obj) {
  $line = ($obj | ConvertTo-Json -Compress -Depth 10)
  Add-Content -Path $log -Value $line -Encoding utf8
  Write-Host $line
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
    $parsed = $null
    try { $parsed = $r.Content | ConvertFrom-Json } catch {}
    return @{ ok = $true; status = [int]$r.StatusCode; raw = $r.Content; json = $parsed }
  } catch {
    $resp = $_.Exception.Response
    $code = 0; $text = $_.Exception.Message
    if ($resp) {
      $code = [int]$resp.StatusCode
      $stream = $resp.GetResponseStream()
      if ($stream) {
        $reader = New-Object System.IO.StreamReader($stream)
        $text = $reader.ReadToEnd()
      }
    }
    return @{ ok = $false; status = $code; raw = $text; json = $null }
  }
}

function Login($path, $user, $pass) {
  $r = Api POST "$base/$path" $null @{ username = $user; password = $pass }
  Ev @{ step = "login_$user"; ok = $r.ok; status = $r.status; snippet = $r.raw.Substring(0, [Math]::Min(180, $r.raw.Length)) }
  if (-not $r.ok) { throw "login failed $user $($r.raw)" }
  return $r.json.accessToken
}

function Snip($s, $n = 500) {
  if ($null -eq $s) { return '' }
  return $s.Substring(0, [Math]::Min($n, $s.Length))
}

$admin = Login 'admin/Auth/login' 'admin' 'admin123'

foreach ($u in @(
  @{ fullName='Employee A'; motherName='N/A'; username='emp_a_04f'; password='EmpA123!'; nationalCode='100000001'; birthDate='1990-01-01'; roleId=7; branchIds=@(1); address='addr-a'; phoneNumber='07700000001' },
  @{ fullName='Employee B'; motherName='N/A'; username='emp_b_04f'; password='EmpB123!'; nationalCode='100000002'; birthDate='1990-01-01'; roleId=7; branchIds=@(1); address='addr-b'; phoneNumber='07700000002' }
)) {
  $cr = Api POST "$base/admin/Users" $admin $u
  Ev @{ step = "create_$($u.username)"; ok = $cr.ok; status = $cr.status; snippet = (Snip $cr.raw 300) }
}

$idsOut = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -F ',' -f "$outDir\_ids.sql"
Ev @{ step = 'user_ids_raw'; idsOut = @($idsOut) }
$empAId = (@($idsOut) | Where-Object { $_ -match ',emp_a_04f$' } | ForEach-Object { ($_ -split ',')[0] } | Select-Object -First 1)
$empBId = (@($idsOut) | Where-Object { $_ -match ',emp_b_04f$' } | ForEach-Object { ($_ -split ',')[0] } | Select-Object -First 1)
if (-not $empAId -or -not $empBId) { throw "missing emp ids A=$empAId B=$empBId out=$idsOut" }
Ev @{ step = 'user_ids'; empAId = $empAId; empBId = $empBId }

$seedOut = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -f "$outDir\_seed_sr.sql"
Ev @{ step = 'seed_sales_request'; seedOut = @($seedOut) }
$reqId = (@($seedOut) | Where-Object { $_ -match '^\d+$' } | Select-Object -Last 1)
if (-not $reqId) { throw "failed to create sales request: $seedOut" }

$assign = Api POST "$base/admin/SalesRequests/$reqId/assign" $admin @{ employeeId = [int]$empAId; reason = '04F assign A' }
Ev @{ step = 'assign_to_A'; ok = $assign.ok; status = $assign.status; body = $assign.raw }

$tokA = Login 'app/Auth/login' 'emp_a_04f' 'EmpA123!'
$tokB = Login 'app/Auth/login' 'emp_b_04f' 'EmpB123!'

$listA = Api GET "$base/app/SalesRequests?tab=incoming" $tokA $null
$listB = Api GET "$base/app/SalesRequests?tab=incoming" $tokB $null
Ev @{ step = 'A_incoming'; ok = $listA.ok; status = $listA.status; body = (Snip $listA.raw) }
Ev @{ step = 'B_incoming_should_empty'; ok = $listB.ok; status = $listB.status; body = (Snip $listB.raw) }

$view = Api POST "$base/app/SalesRequests/$reqId/view" $tokA $null
$prep = Api POST "$base/app/SalesRequests/$reqId/prepare" $tokA @{ note = 'ready' }
Ev @{ step = 'view'; ok = $view.ok; status = $view.status; body = $view.raw }
Ev @{ step = 'prepare'; ok = $prep.ok; status = $prep.status; body = $prep.raw }

$counts1 = Api GET "$base/admin/SalesRequests/counts" $admin $null
$detail1 = Api GET "$base/admin/SalesRequests/$reqId" $admin $null
$timeline1 = Api GET "$base/admin/SalesRequests/$reqId/timeline" $admin $null
Ev @{ step = 'admin_counts_after_prepare'; ok = $counts1.ok; body = (Snip $counts1.raw 800) }
Ev @{ step = 'admin_detail_after_prepare'; ok = $detail1.ok; body = (Snip $detail1.raw 800) }
Ev @{ step = 'admin_timeline_after_prepare'; ok = $timeline1.ok; body = (Snip $timeline1.raw 800) }

$insp = Api POST "$base/app/SalesRequests/$reqId/inspect" $tokA $null
Ev @{ step = 'inspect'; ok = $insp.ok; status = $insp.status; body = $insp.raw }
$d1 = Api POST "$base/app/SalesRequests/$reqId/ensure-draft" $tokA $null
Start-Sleep -Milliseconds 200
$d2 = Api POST "$base/app/SalesRequests/$reqId/ensure-draft" $tokA $null
Ev @{ step = 'ensure_draft_1'; ok = $d1.ok; status = $d1.status; body = $d1.raw }
Ev @{ step = 'ensure_draft_2'; ok = $d2.ok; status = $d2.status; body = $d2.raw }

$linkedSql = 'SELECT "Id", "LinkedOrderId", "Status" FROM "SalesRequests" WHERE "Id"=' + $reqId + '; SELECT COUNT(*) AS order_count FROM "Orders";'
$linkedSql | Set-Content -Path "$outDir\_linked.sql" -Encoding ascii
$linkedCheck = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -F '|' -f "$outDir\_linked.sql"
Ev @{ step = 'linked_order_idempotency'; linkedCheck = @($linkedCheck) }

$re = Api POST "$base/admin/SalesRequests/$reqId/assign" $admin @{ employeeId = [int]$empBId; reason = '04F reassign B' }
Ev @{ step = 'reassign_to_B'; ok = $re.ok; status = $re.status; body = $re.raw }

$prepA2 = Api POST "$base/app/SalesRequests/$reqId/prepare" $tokA @{ note = 'should fail' }
Ev @{ step = 'A_after_reassign_blocked'; ok = $prepA2.ok; status = $prepA2.status; body = $prepA2.raw }

$listA2 = Api GET "$base/app/SalesRequests?tab=incoming" $tokA $null
$listBtabs = @{
  incoming = Api GET "$base/app/SalesRequests?tab=incoming" $tokB $null
  prepared = Api GET "$base/app/SalesRequests?tab=prepared" $tokB $null
  inspected = Api GET "$base/app/SalesRequests?tab=inspected" $tokB $null
}
Ev @{ step = 'A_incoming_after_reassign'; body = (Snip $listA2.raw 300) }
Ev @{ step = 'B_tabs_after_reassign'; incoming = (Snip $listBtabs.incoming.raw 300); prepared = (Snip $listBtabs.prepared.raw 300); inspected = (Snip $listBtabs.inspected.raw 300) }

$exOut = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -f "$outDir\_seed_ex.sql"
$exId = (@($exOut) | Where-Object { $_ -match '^\d+$' } | Select-Object -Last 1)
Ev @{ step = 'exception_request_id'; exId = $exId }
[void](Api POST "$base/admin/SalesRequests/$exId/assign" $admin @{ employeeId = [int]$empAId; reason = 'ex assign' })
$reqEx = Api POST "$base/admin/SalesRequests/$exId/request-exception" $admin @{ reason = 'need exception' }
Ev @{ step = 'request_exception'; ok = $reqEx.ok; status = $reqEx.status; body = $reqEx.raw }
$appr = Api POST "$base/admin/SalesRequests/$exId/approve-exception" $admin @{ reason = 'approved by manager' }
Ev @{ step = 'approve_exception'; ok = $appr.ok; status = $appr.status; body = $appr.raw }
$exDetail = Api GET "$base/admin/SalesRequests/$exId" $admin $null
Ev @{ step = 'exception_detail'; ok = $exDetail.ok; body = (Snip $exDetail.raw 600) }

$delOut = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -f "$outDir\_seed_del.sql"
$delId = (@($delOut) | Where-Object { $_ -match '^\d+$' } | Select-Object -Last 1)
$sd = Api POST "$base/admin/SalesRequests/$delId/soft-delete" $admin @{ reason = '04F soft delete reason' }
$rest = Api POST "$base/admin/SalesRequests/$delId/restore" $admin @{ reason = '04F restore' }
$delList = Api GET "$base/admin/SalesRequests?tab=deleted&pageNumber=1&pageSize=20" $admin $null
Ev @{ step = 'soft_delete'; ok = $sd.ok; status = $sd.status; body = $sd.raw }
Ev @{ step = 'restore'; ok = $rest.ok; status = $rest.status; body = $rest.raw }
Ev @{ step = 'deleted_tab'; ok = $delList.ok; body = (Snip $delList.raw 500) }

$sent = Api GET "$base/admin/SalesRequests?tab=sent&pageNumber=1&pageSize=20" $admin $null
$legal = Api GET "$base/admin/SalesRequests?tab=legal&pageNumber=1&pageSize=20" $admin $null
Ev @{ step = 'sent_tab'; ok = $sent.ok; body = (Snip $sent.raw 500) }
Ev @{ step = 'legal_tab'; ok = $legal.ok; status = $legal.status; body = (Snip $legal.raw 400) }

$firstLine = @($linkedCheck) | Select-Object -First 1
$parts = "$firstLine" -split '\|'
$linkedOrderId = if ($parts.Count -ge 2) { $parts[1] } else { $null }
Ev @{ step = 'parse_linked_order'; firstLine = "$firstLine"; linkedOrderId = $linkedOrderId }

if ($linkedOrderId -and $linkedOrderId -ne '' -and $linkedOrderId -ne '\N') {
  & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -c "UPDATE `"Orders`" SET `"SellerId`"=$empBId WHERE `"Id`"=$linkedOrderId;" | Out-Null
  $complete = Api POST "$base/app/SalesWorkflow/$linkedOrderId/complete" $tokB $null
  Ev @{ step = 'complete_sale'; ok = $complete.ok; status = $complete.status; body = $complete.raw }
  $afterSql = 'SELECT "Id", "LinkedOrderId", "Status" FROM "SalesRequests" WHERE "Id"=' + $reqId + ';'
  $afterSql | Set-Content -Path "$outDir\_after.sql" -Encoding ascii
  $after = & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -t -A -F '|' -f "$outDir\_after.sql"
  Ev @{ step = 'request_after_complete'; after = @($after) }
}

Ev @{ step = 'DONE'; reqId = $reqId; empAId = $empAId; empBId = $empBId; exId = $exId; delId = $delId }
Write-Host 'LIVE_API_SCENARIO_FINISHED'
