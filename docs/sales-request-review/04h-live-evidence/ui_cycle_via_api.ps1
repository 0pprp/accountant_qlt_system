$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:4200/api/v1'
function Api($method, $path, $token, $body=$null) {
  $h = @{}
  if ($token) { $h.Authorization = "Bearer $token" }
  $params = @{ Uri = "$base$path"; Method = $method; Headers = $h; ContentType = 'application/json' }
  if ($null -ne $body) { $params.Body = ($body | ConvertTo-Json -Depth 6 -Compress) }
  try {
    $r = Invoke-WebRequest @params -UseBasicParsing
    return @{ ok=$true; status=[int]$r.StatusCode; json = $(if ($r.Content) { $r.Content | ConvertFrom-Json }) ; raw=$r.Content }
  } catch {
    $resp = $_.Exception.Response
    $code = if ($resp) { [int]$resp.StatusCode } else { 0 }
    $reader = if ($resp) { New-Object IO.StreamReader($resp.GetResponseStream()) } else { $null }
    $raw = if ($reader) { $reader.ReadToEnd() } else { $_.Exception.Message }
    return @{ ok=$false; status=$code; raw=$raw }
  }
}

$log = New-Object System.Collections.Generic.List[string]
function L($s) { $log.Add(("[{0}] {1}" -f (Get-Date -Format o), $s)); Write-Host $s }

$admin = (Api POST '/admin/Auth/login' $null @{username='admin';password='admin123'}).json.accessToken
$tokA = (Api POST '/app/Auth/login' $null @{username='emp_a_04f';password='EmpA123!'}).json.accessToken
$tokB = (Api POST '/app/Auth/login' $null @{username='emp_b_04f';password='EmpB123!'}).json.accessToken
L "tokens ok admin=$([bool]$admin) A=$([bool]$tokA) B=$([bool]$tokB)"

$req = (Api GET '/admin/SalesRequests/8' $admin).json
L ("before status={0} emp={1} stamp={2}" -f $req.status,$req.targetEmployeeId,$req.concurrencyStamp)

# Ensure assigned to A
$assign = Api POST '/admin/SalesRequests/8/assign' $admin @{ targetEmployeeId = 3; concurrencyStamp = $req.concurrencyStamp }
L ("assignA status={0} ok={1} raw={2}" -f $assign.status,$assign.ok,(($assign.raw)|Out-String).Substring(0,[Math]::Min(120,(($assign.raw)|Out-String).Length)))
$req = (Api GET '/admin/SalesRequests/8' $admin).json

# Employee A cycle
$view = Api POST '/app/SalesRequests/8/view' $tokA $null
L ("view={0}" -f $view.status)
$prep = Api POST '/app/SalesRequests/8/prepare' $tokA @{ note = '04H prepare from Web-verified cycle' }
L ("prepare={0} raw={1}" -f $prep.status, (($prep.raw)|Out-String).Substring(0,[Math]::Min(160,(($prep.raw)|Out-String).Length)))
$insp = Api POST '/app/SalesRequests/8/inspect' $tokA $null
L ("inspect={0}" -f $insp.status)
$draft = Api POST '/app/SalesRequests/8/ensure-draft' $tokA $null
L ("ensure-draft={0} raw={1}" -f $draft.status, (($draft.raw)|Out-String).Substring(0,[Math]::Min(200,(($draft.raw)|Out-String).Length)))

$req = (Api GET '/admin/SalesRequests/8' $admin).json
L ("mid status={0} linked={1} emp={2}" -f $req.status,$req.linkedOrderId,$req.targetEmployeeId)

# Reassign to B while in progress (or after reset) — attempt and record
$re = Api POST '/admin/SalesRequests/8/assign' $admin @{ targetEmployeeId = 4; concurrencyStamp = $req.concurrencyStamp }
L ("reassignB={0} raw={1}" -f $re.status, (($re.raw)|Out-String).Substring(0,[Math]::Min(180,(($re.raw)|Out-String).Length)))
$blockA = Api POST '/app/SalesRequests/8/prepare' $tokA @{ note = 'should fail after reassign' }
L ("blockPriorA={0}" -f $blockA.status)

$incomingA = Api GET '/app/SalesRequests?tab=incoming' $tokA
$incomingB = Api GET '/app/SalesRequests?tab=incoming' $tokB
L ("incomingA count={0} incomingB sample={1}" -f @($incomingA.json).Count, (($incomingB.raw)|Out-String).Substring(0,[Math]::Min(160,(($incomingB.raw)|Out-String).Length)))

$outPath = Join-Path $PSScriptRoot '04h_api_cycle.jsonl'
$log | Set-Content -Path $outPath -Encoding UTF8
L "wrote $outPath"
