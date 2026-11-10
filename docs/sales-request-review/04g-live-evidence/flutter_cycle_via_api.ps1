# 04G: دورة موظف A عبر عقود التطبيق (نفس مسارات Flutter) ضد القاعدة المعزولة
# ليس بديلاً عن إثبات واجهة Flutter إن تعذّر الجهاز — يُفصَل في Acceptance.
$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200/api/v1'
$out = 'C:\GitHub\accountant_qlt_system\docs\sales-request-review\04g-live-evidence\flutter_api_cycle.jsonl'
'' | Set-Content $out -Encoding utf8
function Ev($o) { $l = ($o | ConvertTo-Json -Compress -Depth 8); Add-Content $out $l; Write-Host $l }
function Api($Method,$Url,$Token,$Body) {
  $h = @{ 'Content-Type'='application/json' }; if ($Token) { $h.Authorization = "Bearer $Token" }
  $json = if ($null -ne $Body) { $Body | ConvertTo-Json -Compress -Depth 6 } else { $null }
  try {
    if ($json) { $r = Invoke-WebRequest -Uri $Url -Method $Method -Headers $h -Body ([Text.Encoding]::UTF8.GetBytes($json)) -UseBasicParsing }
    else { $r = Invoke-WebRequest -Uri $Url -Method $Method -Headers $h -UseBasicParsing }
    return @{ ok=$true; status=[int]$r.StatusCode; raw=$r.Content; json=($r.Content|ConvertFrom-Json -ErrorAction SilentlyContinue) }
  } catch {
    $resp=$_.Exception.Response; $code=0; $text=$_.Exception.Message
    if ($resp) { $code=[int]$resp.StatusCode; $text=(New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd() }
    return @{ ok=$false; status=$code; raw=$text }
  }
}
$admin = (Api POST "$base/admin/Auth/login" $null @{username='admin';password='admin123'}).json.accessToken
$tokA = (Api POST "$base/app/Auth/login" $null @{username='emp_a_04f';password='EmpA123!'}).json.accessToken
$tokB = (Api POST "$base/app/Auth/login" $null @{username='emp_b_04f';password='EmpB123!'}).json.accessToken
$reqId = 7
$assign = Api POST "$base/admin/SalesRequests/$reqId/assign" $admin @{ employeeId=3; reason='04G assign A for flutter cycle' }
Ev @{ step='assign_A'; ok=$assign.ok; status=$assign.status; body=$assign.raw }
$listA = Api GET "$base/app/SalesRequests?tab=incoming" $tokA $null
$listB = Api GET "$base/app/SalesRequests?tab=incoming" $tokB $null
Ev @{ step='A_sees'; body=$listA.raw.Substring(0,[Math]::Min(400,$listA.raw.Length)) }
Ev @{ step='B_empty'; body=$listB.raw.Substring(0,[Math]::Min(200,$listB.raw.Length)) }
Ev @{ step='view'; result=(Api POST "$base/app/SalesRequests/$reqId/view" $tokA $null) }
Ev @{ step='prepare'; result=(Api POST "$base/app/SalesRequests/$reqId/prepare" $tokA @{ note='04G prepare note long enough for design' }) }
Ev @{ step='inspect'; result=(Api POST "$base/app/SalesRequests/$reqId/inspect" $tokA $null) }
$d1 = Api POST "$base/app/SalesRequests/$reqId/ensure-draft" $tokA $null
$d2 = Api POST "$base/app/SalesRequests/$reqId/ensure-draft" $tokA $null
Ev @{ step='ensure_draft_1'; body=$d1.raw }
Ev @{ step='ensure_draft_2'; body=$d2.raw }
$re = Api POST "$base/admin/SalesRequests/$reqId/assign" $admin @{ employeeId=4; reason='04G reassign B' }
Ev @{ step='reassign_B'; ok=$re.ok; status=$re.status }
$blocked = Api POST "$base/app/SalesRequests/$reqId/prepare" $tokA @{ note='should fail' }
Ev @{ step='A_blocked'; ok=$blocked.ok; status=$blocked.status; body=$blocked.raw }
# complete as B if linked
$linked = ($d2.json.linkedOrderId)
if ($linked) {
  $psql='C:\Program Files\PostgreSQL\18\bin\psql.exe'; $env:PGPASSWORD='postgres'
  & $psql -h localhost -p 5432 -U postgres -d qalaat_04f_sales_verify -c "UPDATE `"Orders`" SET `"SellerId`"=4 WHERE `"Id`"=$linked;" | Out-Null
  $complete = Api POST "$base/app/SalesWorkflow/$linked/complete" $tokB $null
  Ev @{ step='complete'; ok=$complete.ok; status=$complete.status; body=$complete.raw }
}
Ev @{ step='DONE'; reqId=$reqId }
Write-Host 'FLUTTER_API_CYCLE_DONE'
