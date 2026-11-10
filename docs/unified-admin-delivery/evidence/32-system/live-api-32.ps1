$ErrorActionPreference = 'Continue'
$base = 'http://127.0.0.1:15645'
$out = Join-Path $PSScriptRoot 'live-api-32.json'
$results = [ordered]@{
  at = (Get-Date).ToUniversalTime().ToString('o')
  api = $base
  database = 'qalaat_22_23_iso'
  steps = [ordered]@{}
}

function Login($user, $pass) {
  $body = @{ username = $user; password = $pass } | ConvertTo-Json
  return Invoke-RestMethod -Uri "$base/api/v1/admin/Auth/login" -Method Post -Body $body -ContentType 'application/json'
}
function LoginApp($user, $pass) {
  $body = @{ username = $user; password = $pass } | ConvertTo-Json
  return Invoke-RestMethod -Uri "$base/api/v1/app/Auth/login" -Method Post -Body $body -ContentType 'application/json'
}
function AuthHeaders([string]$token) { @{ Authorization = "Bearer $token"; Accept = 'application/json' } }
function Capture($name, $script) {
  try {
    $val = & $script
    $results.steps[$name] = $val
    Write-Host "PASS $name"
  } catch {
    $code = $null
    if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
    $msg = $_.ErrorDetails.Message
    if (-not $msg) { $msg = $_.Exception.Message }
    $results.steps[$name] = @{ error = $true; status = $code; message = ($msg | Out-String).Substring(0, [Math]::Min(500, ($msg | Out-String).Length)) }
    Write-Host "FAIL $name :: $code"
  }
}

$admin = Login 'admin' 'admin123'
$ah = AuthHeaders $admin.accessToken
Capture 'auth.admin' { @{ ok = $true; tokenChars = $admin.accessToken.Length } }

# Probe accounts
foreach ($u in @('follower_iso','mandob_iso','branch_mgr_iso','delegate_iso','sales_iso','motaba_iso','noperm_iso')) {
  Capture "auth.probe.$u" {
    try {
      $t = Login $u 'admin123'
      @{ ok = $true; tokenChars = $t.accessToken.Length }
    } catch {
      $code = $null; if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
      @{ ok = $false; status = $code; message = $_.Exception.Message }
    }
  }
}

# Missing-module probes (read endpoints)
Capture 'mod.waivers.list' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers?page=1&pageSize=5" -Headers $ah
  @{ total = $r.totalCount; items = @($r.items).Count; awaiting = $r.awaitingAccountantCount }
}
Capture 'mod.legalDecisions.list' {
  try {
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/LegalDecisions?page=1&pageSize=5" -Headers $ah
    @{ ok = $true; shape = ($r | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name) -join ',' }
  } catch {
    $code = [int]$_.Exception.Response.StatusCode
    @{ ok = $false; status = $code; message = $_.ErrorDetails.Message }
  }
}
Capture 'mod.priceBoard' {
  try {
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/PriceBoard" -Headers $ah
    @{ ok = $true; keys = ($r.PSObject.Properties.Name -join ',') }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}
Capture 'mod.announcements' {
  try {
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/Announcements?page=1&pageSize=5" -Headers $ah
    @{ ok = $true; total = $r.totalCount }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}
Capture 'mod.tutorialVideos' {
  try {
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/TutorialVideos?page=1&pageSize=5" -Headers $ah
    @{ ok = $true; total = $r.totalCount }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}
Capture 'mod.appNotes' {
  try {
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/AppNotes?page=1&pageSize=5" -Headers $ah
    @{ ok = $true; total = $r.totalCount }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}
Capture 'mod.committeeTasks' {
  try {
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/CommitteeTasks?page=1&pageSize=5" -Headers $ah
    @{ ok = $true; total = $r.totalCount }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}
Capture 'mod.wrongSales' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/WrongSales?page=1&pageSize=5" -Headers $ah
  @{ total = $r.totalCount; items = @($r.items).Count }
}
Capture 'mod.branchVisits.active' {
  try {
    $r = Invoke-WebRequest -Uri "$base/api/v1/admin/BranchVisits/active" -Headers $ah -UseBasicParsing
    @{ status = [int]$r.StatusCode; empty = [string]::IsNullOrWhiteSpace($r.Content) }
  } catch {
    @{ status = [int]$_.Exception.Response.StatusCode }
  }
}
Capture 'mod.branchCustomers' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchCustomers?branchId=1&page=1&pageSize=5" -Headers $ah
  @{ total = $r.totalCount; items = @($r.items).Count }
}
Capture 'mod.branchPayments' {
  try {
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchPayments?branchId=1&page=1&pageSize=5" -Headers $ah
    @{ ok = $true; total = $r.totalCount }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}
Capture 'mod.newSales' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/NewSales?page=1&pageSize=5" -Headers $ah
  @{ total = $r.totalCount; items = @($r.items).Count }
}

# Forbidden without auth
Capture 'authz.noToken.waivers' {
  try {
    Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers?page=1" | Out-Null
    @{ denied = $false }
  } catch {
    @{ denied = $true; status = [int]$_.Exception.Response.StatusCode }
  }
}

# Find an order/customer for waiver financial cycle
Capture 'waiver.pickOrder' {
  $orders = Invoke-RestMethod -Uri "$base/api/v1/admin/Orders?page=1&pageSize=20" -Headers $ah
  $cand = @($orders.items) | Where-Object { $_.sellAmount -gt 100 -or $_.SellAmount -gt 100 } | Select-Object -First 1
  if (-not $cand) {
    # try customers list
    $cust = Invoke-RestMethod -Uri "$base/api/v1/admin/Customers?page=1&pageSize=5" -Headers $ah
    return @{ via = 'customers'; customerId = $cust.items[0].id; orderId = $null; sell = $null; total = $orders.totalCount }
  }
  @{ via = 'orders'; customerId = $cand.customerId; orderId = $cand.id; sell = $(if ($cand.sellAmount) { $cand.sellAmount } else { $cand.SellAmount }); total = $orders.totalCount }
}

$pick = $results.steps['waiver.pickOrder']
if ($pick -and -not $pick.error -and $pick.customerId) {
  Capture 'waiver.create' {
    $body = @{ customerId = [int]$pick.customerId; note = 'Batch32 waiver reject path then apply path'; orderId = $pick.orderId } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers" -Method Post -Headers $ah -Body $body -ContentType 'application/json'
    @{ id = $r.id; status = $r.status; version = $r.expectedVersion; remaining = $r.remainingAmount }
  }
  $w = $results.steps['waiver.create']
  if ($w -and $w.id) {
    Capture 'waiver.reject' {
      $body = @{ expectedVersion = [int]$w.version; note = 'reject cycle 32' } | ConvertTo-Json
      $r = Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers/$($w.id)/reject" -Method Post -Headers $ah -Body $body -ContentType 'application/json'
      @{ id = $r.id; status = $r.status; version = $r.expectedVersion }
    }
  }

  Capture 'waiver.create2' {
    $body = @{ customerId = [int]$pick.customerId; note = 'Batch32 apply financial path'; orderId = $pick.orderId } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers" -Method Post -Headers $ah -Body $body -ContentType 'application/json'
    @{ id = $r.id; status = $r.status; version = $r.expectedVersion; remainingBefore = $r.remainingAmount; sellBefore = $r.orderSellAmount }
  }
  $w2 = $results.steps['waiver.create2']
  if ($w2 -and $w2.id) {
    $approveAmt = 50
    Capture 'waiver.approve.delegate' {
      $body = @{ expectedVersion = [int]$w2.version; approvedAmount = $approveAmt; note = 'await accountant' } | ConvertTo-Json
      $r = Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers/$($w2.id)/approve" -Method Post -Headers $ah -Body $body -ContentType 'application/json'
      @{ id = $r.id; status = $r.status; version = $r.expectedVersion; approved = $r.approvedAmount; sell = $r.orderSellAmount }
    }
    $w2a = $results.steps['waiver.approve.delegate']
    Capture 'waiver.preApply.noFinancialYet' {
      $detail = Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers/$($w2.id)" -Headers $ah
      @{ status = $detail.status; sell = $detail.orderSellAmount; approved = $detail.approvedAmount; equalToCreate = ($detail.orderSellAmount -eq $w2.sellBefore -or $null -eq $w2.sellBefore) }
    }
    if ($w2a -and -not $w2a.error) {
      Capture 'waiver.apply' {
        $body = @{ expectedVersion = [int]$w2a.version } | ConvertTo-Json
        $r = Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers/$($w2.id)/apply" -Method Post -Headers $ah -Body $body -ContentType 'application/json'
        @{ id = $r.id; status = $r.status; version = $r.expectedVersion; sellAfter = $r.orderSellAmount; approved = $r.approvedAmount }
      }
      Capture 'waiver.reapply.idempotent' {
        try {
          $body = @{ expectedVersion = [int]$results.steps['waiver.apply'].version } | ConvertTo-Json
          $r = Invoke-RestMethod -Uri "$base/api/v1/admin/Waivers/$($w2.id)/apply" -Method Post -Headers $ah -Body $body -ContentType 'application/json'
          @{ unexpectedSuccess = $true; status = $r.status; sell = $r.orderSellAmount }
        } catch {
          @{ deniedOrConflict = $true; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
        }
      }
    }
  }
}

# App notes create from app if mandob works
Capture 'app.notes.write' {
  try {
    $m = LoginApp 'mandob_iso' 'admin123'
    $mh = AuthHeaders $m.accessToken
    $body = @{ note = 'batch32 mandob note from app'; customerId = 1 } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$base/api/v1/app/Notes" -Method Post -Headers $mh -Body $body -ContentType 'application/json'
    @{ ok = $true; id = $r.id }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}

Capture 'app.wrongSale.firstCollectionEval' {
  try {
    $m = LoginApp 'mandob_iso' 'admin123'
    $mh = AuthHeaders $m.accessToken
    $body = @{ orderListId = 1; score = 3; note = 'batch32 first collection eval' } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$base/api/v1/app/WrongSales/first-collection-eval" -Method Post -Headers $mh -Body $body -ContentType 'application/json'
    @{ ok = $true; keys = ($r.PSObject.Properties.Name -join ','); noticeCreated = $r.noticeCreated }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}

# Sales requests list
Capture 'salesRequests.unassigned' {
  try {
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/SalesRequests?status=Unassigned&page=1&pageSize=5" -Headers $ah
    @{ ok = $true; total = $r.totalCount }
  } catch {
    @{ ok = $false; status = [int]$_.Exception.Response.StatusCode; message = $_.ErrorDetails.Message }
  }
}

$results | ConvertTo-Json -Depth 8 | Out-File $out -Encoding utf8
Write-Host "WROTE $out"
$pass = @($results.steps.GetEnumerator() | Where-Object { -not $_.Value.error -and -not ($_.Value.ok -eq $false) }).Count
$fail = @($results.steps.GetEnumerator() | Where-Object { $_.Value.error -or $_.Value.ok -eq $false }).Count
Write-Host "SUMMARY passish=$pass failish=$fail total=$($results.steps.Count)"
