$ErrorActionPreference = 'Continue'
$base = 'http://127.0.0.1:15645'
$out = Join-Path $PSScriptRoot 'live-api.json'
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
    $results.steps[$name] = @{ error = $true; status = $code; message = $msg }
    Write-Host "FAIL $name :: $code $msg"
  }
}

# --- admin login (after branch2 scoped) ---
$admin = Login 'admin' 'admin123'
$ah = AuthHeaders $admin.accessToken

Capture 'admin.claims.branchIds' {
  $payload = $admin.accessToken.Split('.')[1]
  while ($payload.Length % 4) { $payload += '=' }
  $json = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload.Replace('-','+').Replace('_','/')))
  $obj = $json | ConvertFrom-Json
  @{ branchIds = $obj.branchIds; role = $obj.'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' }
}

# Active visit (may already be open from prior step)
Capture 'visit.active.before' {
  try {
    $r = Invoke-WebRequest -Uri "$base/api/v1/admin/BranchVisits/active" -Headers $ah -UseBasicParsing
    if ($r.StatusCode -eq 204 -or [string]::IsNullOrWhiteSpace($r.Content)) { return @{ active = $null } }
    return ($r.Content | ConvertFrom-Json)
  } catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 204) { return @{ active = $null } }
    throw
  }
}

$visitId = $null
$ver = 0
if ($results.steps['visit.active.before'] -and $results.steps['visit.active.before'].id) {
  $visitId = $results.steps['visit.active.before'].id
  $ver = $results.steps['visit.active.before'].expectedVersion
}

# If open, rate+depart first so we can reopen with members
if ($visitId) {
  Capture 'visit.rate' {
    $body = @{ orderListId = 1; score = 8; expectedVersion = $ver } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/$visitId/ratings" -Method Put -Headers $ah -Body $body -ContentType 'application/json'
    $script:ver = $r.expectedVersion
    @{ id = $r.id; ratings = $r.ratings; expectedVersion = $r.expectedVersion }
  }

  Capture 'visit.depart.bad-file' {
    $note = ('ملاحظة مغادرة تحقق حي للدفعة 31 — يجب ألا تقل عن خمسين حرفًا. ' + ('x' * 20))
    $boundary = [guid]::NewGuid().ToString('N')
    $fileBytes = [Text.Encoding]::UTF8.GetBytes('not-an-image')
    $sb = New-Object Text.StringBuilder
    [void]$sb.AppendLine("--$boundary")
    [void]$sb.AppendLine('Content-Disposition: form-data; name="note"')
    [void]$sb.AppendLine()
    [void]$sb.AppendLine($note)
    [void]$sb.AppendLine("--$boundary")
    [void]$sb.AppendLine('Content-Disposition: form-data; name="expectedVersion"')
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("$script:ver")
    [void]$sb.AppendLine("--$boundary")
    [void]$sb.AppendLine('Content-Disposition: form-data; name="files"; filename="evil.exe"')
    [void]$sb.AppendLine('Content-Type: application/octet-stream')
    [void]$sb.AppendLine()
    $prefix = [Text.Encoding]::UTF8.GetBytes($sb.ToString())
    $suffix = [Text.Encoding]::UTF8.GetBytes("`r`n--$boundary--`r`n")
    $body = New-Object byte[] ($prefix.Length + $fileBytes.Length + $suffix.Length)
    [Buffer]::BlockCopy($prefix, 0, $body, 0, $prefix.Length)
    [Buffer]::BlockCopy($fileBytes, 0, $body, $prefix.Length, $fileBytes.Length)
    [Buffer]::BlockCopy($suffix, 0, $body, $prefix.Length + $fileBytes.Length, $suffix.Length)
    try {
      Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/$visitId/depart" -Method Post -Headers $ah -ContentType "multipart/form-data; boundary=$boundary" -Body $body | Out-Null
      return @{ unexpected = 'success' }
    } catch {
      return @{ status = [int]$_.Exception.Response.StatusCode; body = $_.ErrorDetails.Message }
    }
  }

  Capture 'visit.depart.ok' {
    $note = ('ملاحظة مغادرة تحقق حي للدفعة 31 مع مرفق صورة صالح وإثبات أن المغادرة لا تنجح عند ملف مرفوض. ' + ('ن' * 10))
    $png = [Convert]::FromBase64String('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==')
    $boundary = [guid]::NewGuid().ToString('N')
    $ms = New-Object IO.MemoryStream
    $w = New-Object IO.StreamWriter($ms)
    $w.Write("--$boundary`r`n")
    $w.Write("Content-Disposition: form-data; name=`"note`"`r`n`r`n$note`r`n")
    $w.Write("--$boundary`r`n")
    $w.Write("Content-Disposition: form-data; name=`"expectedVersion`"`r`n`r`n$script:ver`r`n")
    $w.Write("--$boundary`r`n")
    $w.Write("Content-Disposition: form-data; name=`"files`"; filename=`" depart.png`"`r`n")
    $w.Write("Content-Type: image/png`r`n`r`n")
    $w.Flush()
    $ms.Write($png, 0, $png.Length)
    $end = [Text.Encoding]::UTF8.GetBytes("`r`n--$boundary--`r`n")
    $ms.Write($end, 0, $end.Length)
    $bytes = $ms.ToArray()
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/$visitId/depart" -Method Post -Headers (@{ Authorization = $ah.Authorization }) -ContentType "multipart/form-data; boundary=$boundary" -Body $bytes
    $noteLen = 0; if ($r.departNote) { $noteLen = ([string]$r.departNote).Length }
    @{ id = $r.id; status = $r.status; attachments = $r.attachments; departNoteLen = $noteLen; expectedVersion = $r.expectedVersion }
  }

  Capture 'visit.depart.idempotent' {
    $note = ('ملاحظة مغادرة تحقق حي للدفعة 31 مع مرفق صورة صالح وإثبات أن المغادرة لا تنجح عند ملف مرفوض. ' + ('ن' * 10))
    $boundary = [guid]::NewGuid().ToString('N')
    $bodyText = @"
--$boundary
Content-Disposition: form-data; name="note"

$note
--$boundary
Content-Disposition: form-data; name="expectedVersion"

0
--$boundary--
"@
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/$visitId/depart" -Method Post -Headers (@{ Authorization = $ah.Authorization }) -ContentType "multipart/form-data; boundary=$boundary" -Body ([Text.Encoding]::UTF8.GetBytes($bodyText.Replace("`n","`r`n")))
    @{ id = $r.id; status = $r.status; attachmentsCount = @($r.attachments).Count }
  }
}

# Re-open same branch — members should include bm_iso
Capture 'visit.open.first-after-bm' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/open" -Method Post -Headers $ah -Body '{"branchId":1}' -ContentType 'application/json'
  $script:visitId = $r.id
  $script:ver = $r.expectedVersion
  @{
    id = $r.id
    status = $r.status
    members = $r.members
    assignedTasks = $r.assignedTasks
    expectedVersion = $r.expectedVersion
  }
}

Capture 'visit.open.idempotent-same' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/open" -Method Post -Headers $ah -Body '{"branchId":1}' -ContentType 'application/json'
  @{ id = $r.id; sameId = ($r.id -eq $script:visitId); membersCount = @($r.members).Count; tasksCount = @($r.assignedTasks).Count }
}

Capture 'visit.open.other-branch-AlreadyOpen' {
  try {
    Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/open" -Method Post -Headers $ah -Body '{"branchId":2}' -ContentType 'application/json' | Out-Null
    @{ unexpected = 'success' }
  } catch {
    @{ status = [int]$_.Exception.Response.StatusCode; body = $_.ErrorDetails.Message }
  }
}

Capture 'visit.open.out-of-scope' {
  try {
    Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/open" -Method Post -Headers $ah -Body '{"branchId":999}' -ContentType 'application/json' | Out-Null
    @{ unexpected = 'success' }
  } catch {
    @{ status = [int]$_.Exception.Response.StatusCode; body = $_.ErrorDetails.Message }
  }
}

# Rate + depart clean for delegate view
Capture 'visit.rate2' {
  $body = @{ orderListId = 1; score = 7; expectedVersion = $script:ver } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/$($script:visitId)/ratings" -Method Put -Headers $ah -Body $body -ContentType 'application/json'
  $script:ver = $r.expectedVersion
  @{ ratings = $r.ratings; expectedVersion = $r.expectedVersion }
}

Capture 'visit.depart2' {
  $note = ('مغادرة ثانية بعد إثبات الأعضاء والمهام — ملاحظة كافية الطول للتحقق من مسار المفوض. ' + ('م' * 15))
  $boundary = [guid]::NewGuid().ToString('N')
  $bodyText = @"
--$boundary
Content-Disposition: form-data; name="note"

$note
--$boundary
Content-Disposition: form-data; name="expectedVersion"

$($script:ver)
--$boundary--
"@
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits/$($script:visitId)/depart" -Method Post -Headers (@{ Authorization = $ah.Authorization }) -ContentType "multipart/form-data; boundary=$boundary" -Body ([Text.Encoding]::UTF8.GetBytes($bodyText.Replace("`n","`r`n")))
  @{ id = $r.id; status = $r.status; members = $r.members; attachmentsCount = @($r.attachments).Count }
}

Capture 'visit.list.for-delegate' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchVisits?page=1&pageSize=20" -Headers $ah
  @{ total = $r.totalCount; items = @($r.items | Select-Object id, branchId, status, openedByName, @{n='members';e={@($_.members).Count}}, @{n='attachments';e={@($_.attachments).Count}}) }
}

# NewSales
Capture 'newsales.list' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchNewSales" -Headers $ah
  @{ total = $r.totalCount; unread = $r.unreadCount; first = ($r.items | Select-Object -First 1) }
}

Capture 'newsales.mark-read' {
  $list = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchNewSales" -Headers $ah
  if (-not $list.items -or $list.items.Count -eq 0) { return @{ skipped = 'no items' } }
  $id = $list.items[0].id
  $beforeUnread = $list.unreadCount
  $detail = $null
  try { $detail = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchNewSales/$id" -Headers $ah } catch { $detail = @{ error = $_.ErrorDetails.Message } }
  # auto-read on details if implemented; also explicit mark-read
  try { Invoke-RestMethod -Uri "$base/api/v1/admin/BranchNewSales/$id/mark-read" -Method Post -Headers $ah | Out-Null } catch { }
  $after = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchNewSales" -Headers $ah
  $status = $null
  if ($detail -is [hashtable]) { $status = $detail.workflowStatus } else { $status = $detail.workflowStatus }
  if (-not $status -and $list.items[0].workflowStatus) { $status = $list.items[0].workflowStatus }
  @{
    id = $id
    detail = $detail
    detailStatus = $status
    beforeUnread = $beforeUnread
    afterUnread = $after.unreadCount
    commercialUnchanged = ($status -eq 'Sold' -or $status -eq 5 -or "$status" -match 'Sold')
  }
}

# Customers / payments
Capture 'customers.search' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/BranchCustomers/search?text=%D8%A7%D8%AD" -Headers $ah
  @{ count = @($r.items).Count; items = $r.items }
}

Capture 'payments.sum' {
  $from = (Get-Date).AddDays(-40).ToString('yyyy-MM-dd')
  $to = (Get-Date).ToString('yyyy-MM-dd')
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/ManagementRatings/list-payments-sum?listId=1&from=$from&to=$to" -Headers $ah
  $r
}

Capture 'ratings.catalog' {
  Invoke-RestMethod -Uri "$base/api/v1/admin/ManagementRatings/catalog?branchId=1" -Headers $ah
}

# WrongSale from mandob via app login
Capture 'wrongSale.mandob.create' {
  $m = LoginApp 'mandob_iso' 'admin123'
  $mh = AuthHeaders $m.accessToken
  $body = @{
    customerId = 1
    listId = 1
    rating = 5
    note = 'محل ضعيف التحقق من مسار المندوب'
    shopStockEstimatedValue = 100000
    estimatedDailyRevenue = 15000
    saleAmount = 500000
  } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$base/api/v1/app/WrongSales/first-collection-eval" -Method Post -Headers $mh -Body $body -ContentType 'application/json'
  @{ id = $r.id; sourceApp = $r.sourceApp; status = $r.status; orderId = $r.orderId; followerName = $r.followerName; createdBy = $r.createdByUserId }
}

Capture 'wrongSale.mandob.dedupe' {
  $m = LoginApp 'mandob_iso' 'admin123'
  $mh = AuthHeaders $m.accessToken
  $body = @{
    customerId = 1
    listId = 1
    rating = 4
    note = 'إعادة محاولة لا يجب أن تكرر البلاغ'
    shopStockEstimatedValue = 100000
    estimatedDailyRevenue = 15000
  } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$base/api/v1/app/WrongSales/first-collection-eval" -Method Post -Headers $mh -Body $body -ContentType 'application/json'
  @{ id = $r.id; sourceApp = $r.sourceApp }
}

Capture 'wrongSale.mailbox' {
  $r = Invoke-RestMethod -Uri "$base/api/v1/admin/WrongSales?page=1&pageSize=20" -Headers $ah
  @{ total = $r.totalCount; unread = $r.unreadCount; unreviewed = $r.unreviewedCount; first = ($r.items | Select-Object -First 1) }
}

Capture 'wrongSale.mark-read-review' {
  $list = Invoke-RestMethod -Uri "$base/api/v1/admin/WrongSales?page=1&pageSize=5" -Headers $ah
  if (-not $list.items -or $list.items.Count -eq 0) { return @{ skipped = $true } }
  $id = $list.items[0].id
  $ver = 0
  if ($null -ne $list.items[0].expectedVersion) { $ver = $list.items[0].expectedVersion }
  $read = $null
  try {
    $read = Invoke-RestMethod -Uri "$base/api/v1/admin/WrongSales/$id/mark-delegate-read" -Method Post -Headers $ah -Body (@{ expectedVersion = $ver } | ConvertTo-Json) -ContentType 'application/json'
    if ($null -ne $read.expectedVersion) { $ver = $read.expectedVersion }
  } catch { $read = $_.ErrorDetails.Message }
  $rev = $null
  try {
    $rev = Invoke-RestMethod -Uri "$base/api/v1/admin/WrongSales/$id/mark-branch-reviewed" -Method Post -Headers $ah -Body (@{ expectedVersion = $ver; note = 'مراجعة فرع iso' } | ConvertTo-Json) -ContentType 'application/json'
  } catch { $rev = $_.ErrorDetails.Message }
  $conflict = $null
  try {
    Invoke-RestMethod -Uri "$base/api/v1/admin/WrongSales/$id/mark-branch-reviewed" -Method Post -Headers $ah -Body (@{ expectedVersion = 0; note = 'تعارض' } | ConvertTo-Json) -ContentType 'application/json' | Out-Null
    $conflict = 'unexpected-success'
  } catch {
    $conflict = @{ status = [int]$_.Exception.Response.StatusCode; body = $_.ErrorDetails.Message }
  }
  @{ id = $id; read = $read; review = $rev; conflictRetry = $conflict }
}

Capture 'wrongSale.out-of-scope-user' {
  # follower has Motaba — try admin WrongSales with empty? Use login then open branch2-only if possible
  try {
    $f = Login 'follower_iso' 'admin123'
    $fh = AuthHeaders $f.accessToken
    $r = Invoke-RestMethod -Uri "$base/api/v1/admin/WrongSales?page=1&pageSize=20" -Headers $fh
    @{ total = $r.totalCount; note = 'follower mailbox within own scope' }
  } catch {
    @{ status = [int]$_.Exception.Response.StatusCode; body = $_.ErrorDetails.Message }
  }
}

$results | ConvertTo-Json -Depth 10 | Set-Content -Path $out -Encoding UTF8
Write-Host "WROTE $out"
