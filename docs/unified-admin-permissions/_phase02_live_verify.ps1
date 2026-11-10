# Phase 02 live API verification — qalaat_phase02_verify only
$ErrorActionPreference = 'Stop'
$Base = 'http://127.0.0.1:4200/api/v1'
$Results = [System.Collections.Generic.List[object]]::new()
# API serializes PermissionOverrideMode as int (no JsonStringEnumConverter)
$ModeInherit = 0; $ModeAllow = 1; $ModeDeny = 2

function Add-Result($id, $status, $detail) {
  $Results.Add([pscustomobject]@{ Id = $id; Status = $status; Detail = $detail })
  Write-Host ("[{0}] {1}: {2}" -f $status, $id, $detail)
}

function Invoke-Api {
  param([string]$Method, [string]$Path, [object]$Body = $null, [string]$Token = $null)
  $uri = "$Base$Path"
  $hdrs = @{}
  if ($Token) { $hdrs['Authorization'] = "Bearer $Token" }
  $params = @{ Method = $Method; Uri = $uri; Headers = $hdrs }
  if ($null -ne $Body) {
    $params['ContentType'] = 'application/json; charset=utf-8'
    $params['Body'] = ($Body | ConvertTo-Json -Depth 10 -Compress)
  }
  try {
    $resp = Invoke-WebRequest @params -UseBasicParsing
    $parsed = $null
    if ($resp.Content) { $parsed = $resp.Content | ConvertFrom-Json }
    return @{ Ok = $true; Status = [int]$resp.StatusCode; Body = $parsed }
  } catch {
    $status = 0; $parsed = $null
    if ($_.Exception.Response) {
      $status = [int]$_.Exception.Response.StatusCode
      $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
      $raw = $reader.ReadToEnd(); $reader.Close()
      if ($raw) { try { $parsed = $raw | ConvertFrom-Json } catch { $parsed = $raw } }
    }
    return @{ Ok = $false; Status = $status; Body = $parsed; Error = $_.Exception.Message }
  }
}

function Test-HasNamedPerm($loginBody, $fullName) {
  $scope, $action = $fullName.Split('.')
  $list = $loginBody.permissions.$scope
  if (-not $list) { return $false }
  return [bool]($list | Where-Object { $_.name -eq $action })
}

function New-DummyUser($username, $roleId, $token, $branchId) {
  return Invoke-Api -Method POST -Path '/admin/Users' -Token $token -Body @{
    fullName = $username; motherName = 'ام تجريبية'; username = $username; password = 'Pass1234!'
    nationalCode = ('N' + (Get-Random -Minimum 100000 -Maximum 999999))
    birthDate = '1990-01-01'; roleId = [int]$roleId; branchIds = @([int]$branchId)
    address = 'عنوان وهمي'; phoneNumber = ('07' + (Get-Random -Minimum 100000000 -Maximum 999999999))
  }
}

# Reset: use unique usernames per run
$suffix = Get-Random -Minimum 1000 -Maximum 9999
$mandobUser = "mandob_p02_$suffix"
$chiefUser = "chief_p02_$suffix"
$newUser = "newuser_p02_$suffix"

$adminLogin = Invoke-Api -Method POST -Path '/admin/Auth/login' -Body @{ username = 'admin'; password = 'admin123' }
if (-not $adminLogin.Ok) { Add-Result 'login-superadmin' 'FAIL' "$($adminLogin.Status)"; $Results | ConvertTo-Json | Set-Content 'C:\GitHub\accountant_qlt_system\docs\unified-admin-permissions\_phase02_live_results.json'; exit 1 }
$adminToken = $adminLogin.Body.accessToken
Add-Result 'login-superadmin' 'PASS' 'ok'

$mp = Test-HasNamedPerm $adminLogin.Body 'User.ManagePermissions'
# ManagePermissions may appear under User scope as ManagePermissions
if (-not $mp) {
  $list = $adminLogin.Body.permissions.User
  $mp = [bool]($list | Where-Object { $_.name -eq 'ManagePermissions' })
}
Add-Result 'seed-manage-permissions' $(if ($mp) { 'PASS' } else { 'FAIL' }) "present=$mp"

$provinces = Invoke-Api -Method GET -Path '/admin/Provinces?pageNumber=1&pageSize=10' -Token $adminToken
$provinceId = $provinces.Body.items[0].id
$branches = Invoke-Api -Method GET -Path '/admin/Branches?pageNumber=1&pageSize=20' -Token $adminToken
$branchId = $null
foreach ($b in @($branches.Body.items)) {
  if ($b.name -eq 'فرع تجريبي') { $branchId = $b.id; break }
}
if (-not $branchId) {
  $null = Invoke-Api -Method POST -Path '/admin/Branches' -Token $adminToken -Body @{ name = 'فرع تجريبي'; provinceId = [int]$provinceId }
  $branches = Invoke-Api -Method GET -Path '/admin/Branches?pageNumber=1&pageSize=20' -Token $adminToken
  foreach ($b in @($branches.Body.items)) { if ($b.name -eq 'فرع تجريبي') { $branchId = $b.id; break } }
}
if (-not $branchId -and $branches.Body.items.Count -gt 0) { $branchId = $branches.Body.items[0].id }
Add-Result 'dummy-branch' $(if ($branchId) { 'PASS' } else { 'FAIL' }) "branchId=$branchId"

$roles = Invoke-Api -Method GET -Path '/admin/Roles' -Token $adminToken
$roleMap = @{}
foreach ($r in @($roles.Body)) { $roleMap[$r.name] = [int]$r.id }
$mandobRoleId = $roleMap['Mandob']; $chiefRoleId = $roleMap['ChiefAccountant']; $adminRoleId = $roleMap['Admin']
Add-Result 'roles-loaded' $(if ($mandobRoleId -and $chiefRoleId) { 'PASS' } else { 'FAIL' }) "Mandob=$mandobRoleId Chief=$chiefRoleId"

$mandob = New-DummyUser $mandobUser $mandobRoleId $adminToken $branchId
$mandobId = [int]$mandob.Body.id
Add-Result 'create-mandob' $(if ($mandob.Ok -and $mandobId) { 'PASS' } else { 'FAIL' }) "id=$mandobId status=$($mandob.Status)"

$chief = New-DummyUser $chiefUser $chiefRoleId $adminToken $branchId
$chiefId = [int]$chief.Body.id
Add-Result 'create-chief' $(if ($chief.Ok -and $chiefId) { 'PASS' } else { 'FAIL' }) "id=$chiefId"

$ov = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken
Add-Result 'open-overrides-editor' $(if ($ov.Ok) { 'PASS' } else { 'FAIL' }) "v=$($ov.Body.permissionsVersion) canEdit=$($ov.Body.canEditOverrides)"

$orderReadId = [int]($ov.Body.rows | Where-Object { $_.name -eq 'Order.Read' } | Select-Object -First 1).permissionId
$userCreateId = [int]($ov.Body.rows | Where-Object { $_.name -eq 'User.Create' } | Select-Object -First 1).permissionId
$productReadId = [int]($ov.Body.rows | Where-Object { $_.name -eq 'Product.Read' } | Select-Object -First 1).permissionId
Add-Result 'resolve-permission-ids' $(if ($orderReadId -and $userCreateId -and $productReadId) { 'PASS' } else { 'FAIL' }) "Order.Read=$orderReadId User.Create=$userCreateId Product.Read=$productReadId"

$mLogin1 = Invoke-Api -Method POST -Path '/admin/Auth/login' -Body @{ username = $mandobUser; password = 'Pass1234!' }
$mToken1 = $mLogin1.Body.accessToken
$mRefresh1 = $mLogin1.Body.refreshToken
$hasOrderBefore = Test-HasNamedPerm $mLogin1.Body 'Order.Read'
Add-Result 'mandob-login-before-deny' $(if ($mLogin1.Ok -and $hasOrderBefore) { 'PASS' } else { 'FAIL' }) "hasOrderRead=$hasOrderBefore"

$ver = [int]$ov.Body.permissionsVersion
$deny = Invoke-Api -Method PATCH -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken -Body @{
  expectedVersion = $ver
  changes = @(@{ permissionId = $orderReadId; mode = $ModeDeny })
}
Add-Result 'deny-inherited' $(if ($deny.Ok) { 'PASS' } else { 'FAIL' }) "status=$($deny.Status)"

$refreshOld = Invoke-Api -Method POST -Path '/admin/Auth/refresh-token' -Body @{ refreshToken = $mRefresh1 }
Add-Result 'session-invalidate-after-override' $(if (-not $refreshOld.Ok) { 'PASS' } else { 'FAIL' }) "refreshStatus=$($refreshOld.Status)"

$mLogin2 = Invoke-Api -Method POST -Path '/admin/Auth/login' -Body @{ username = $mandobUser; password = 'Pass1234!' }
$hasOrderAfter = Test-HasNamedPerm $mLogin2.Body 'Order.Read'
Add-Result 'deny-effective-after-relogin' $(if ($mLogin2.Ok -and -not $hasOrderAfter) { 'PASS' } else { 'FAIL' }) "hasOrderReadAfter=$hasOrderAfter"

$ov2 = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken
$allow = Invoke-Api -Method PATCH -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken -Body @{
  expectedVersion = [int]$ov2.Body.permissionsVersion
  changes = @(@{ permissionId = $userCreateId; mode = $ModeAllow })
}
Add-Result 'allow-extra' $(if ($allow.Ok) { 'PASS' } else { 'FAIL' }) "status=$($allow.Status)"

$mLogin3 = Invoke-Api -Method POST -Path '/admin/Auth/login' -Body @{ username = $mandobUser; password = 'Pass1234!' }
$hasUserCreate = Test-HasNamedPerm $mLogin3.Body 'User.Create'
Add-Result 'allow-effective-after-relogin' $(if ($mLogin3.Ok -and $hasUserCreate) { 'PASS' } else { 'FAIL' }) "hasUserCreate=$hasUserCreate"

$ov3 = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken
$inherit = Invoke-Api -Method PATCH -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken -Body @{
  expectedVersion = [int]$ov3.Body.permissionsVersion
  changes = @(@{ permissionId = $orderReadId; mode = $ModeInherit })
}
Add-Result 'inherit-restore' $(if ($inherit.Ok) { 'PASS' } else { 'FAIL' }) "status=$($inherit.Status)"

$mLogin4 = Invoke-Api -Method POST -Path '/admin/Auth/login' -Body @{ username = $mandobUser; password = 'Pass1234!' }
$hasOrderRestored = Test-HasNamedPerm $mLogin4.Body 'Order.Read'
Add-Result 'inherit-effective-after-relogin' $(if ($mLogin4.Ok -and $hasOrderRestored) { 'PASS' } else { 'FAIL' }) "hasOrderReadRestored=$hasOrderRestored"

$chiefLogin = Invoke-Api -Method POST -Path '/admin/Auth/login' -Body @{ username = $chiefUser; password = 'Pass1234!' }
$chiefToken = $chiefLogin.Body.accessToken
$ovChief = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $chiefToken
Add-Result 'reject-non-super-manage' $(if (-not $ovChief.Ok -and $ovChief.Status -in 401,403) { 'PASS' } else { 'FAIL' }) "status=$($ovChief.Status)"

$updateAsChief = Invoke-Api -Method PUT -Path "/admin/Users/$mandobId" -Token $chiefToken -Body @{
  fullName = $mandobUser; motherName = 'ام تجريبية'; username = $mandobUser
  nationalCode = 'N999888'; birthDate = '1990-01-01'; roleId = $adminRoleId
  branchIds = @([int]$branchId); address = 'عنوان وهمي'; phoneNumber = '07000000001'
}
Add-Result 'reject-role-change-non-super' $(if (-not $updateAsChief.Ok) { 'PASS' } else { 'FAIL' }) "status=$($updateAsChief.Status) body=$($updateAsChief.Body | ConvertTo-Json -Compress)"

# Conflict: two admins
$admin2 = Invoke-Api -Method POST -Path '/admin/Auth/login' -Body @{ username = 'admin'; password = 'admin123' }
$admin2Token = $admin2.Body.accessToken
$ovA = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken
$ovB = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $admin2Token
$vShared = [int]$ovA.Body.permissionsVersion
$patchA = Invoke-Api -Method PATCH -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken -Body @{
  expectedVersion = $vShared
  changes = @(@{ permissionId = $productReadId; mode = $ModeDeny })
}
$patchB = Invoke-Api -Method PATCH -Path "/admin/Users/$mandobId/permission-overrides" -Token $admin2Token -Body @{
  expectedVersion = $vShared
  changes = @(@{ permissionId = $userCreateId; mode = $ModeInherit })
}
Add-Result 'conflict-409' $(if ($patchA.Ok -and -not $patchB.Ok -and $patchB.Status -eq 409) { 'PASS' } else { 'FAIL' }) "A=$($patchA.Status) B=$($patchB.Status)"

$ovAfter = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $admin2Token
$merge = Invoke-Api -Method PATCH -Path "/admin/Users/$mandobId/permission-overrides" -Token $admin2Token -Body @{
  expectedVersion = [int]$ovAfter.Body.permissionsVersion
  changes = @(@{ permissionId = $userCreateId; mode = $ModeInherit })
}
$ovMerged = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $admin2Token
$productMode = [int]($ovMerged.Body.rows | Where-Object { $_.permissionId -eq $productReadId }).mode
$userCreateMode = [int]($ovMerged.Body.rows | Where-Object { $_.permissionId -eq $userCreateId }).mode
Add-Result 'conflict-merge-both-sides' $(if ($merge.Ok -and $productMode -eq $ModeDeny -and $userCreateMode -eq $ModeInherit) { 'PASS' } else { 'FAIL' }) "productMode=$productMode userCreateMode=$userCreateMode"

$verBeforeNoop = [int]$ovMerged.Body.permissionsVersion
$mBeforeNoop = Invoke-Api -Method POST -Path '/admin/Auth/login' -Body @{ username = $mandobUser; password = 'Pass1234!' }
$refreshBefore = $mBeforeNoop.Body.refreshToken
$noop = Invoke-Api -Method PATCH -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken -Body @{
  expectedVersion = $verBeforeNoop
  changes = @(@{ permissionId = $productReadId; mode = $ModeDeny })
}
$ovNoop = Invoke-Api -Method GET -Path "/admin/Users/$mandobId/permission-overrides" -Token $adminToken
$verAfterNoop = [int]$ovNoop.Body.permissionsVersion
$refreshAfterNoop = Invoke-Api -Method POST -Path '/admin/Auth/refresh-token' -Body @{ refreshToken = $refreshBefore }
Add-Result 'noop-no-version-bump' $(if ($noop.Ok -and $verAfterNoop -eq $verBeforeNoop) { 'PASS' } else { 'FAIL' }) "before=$verBeforeNoop after=$verAfterNoop"
Add-Result 'noop-no-session-invalidate' $(if ($refreshAfterNoop.Ok) { 'PASS' } else { 'FAIL' }) "refreshStatus=$($refreshAfterNoop.Status)"

$created = New-DummyUser $newUser $mandobRoleId $adminToken $branchId
$newId = [int]$created.Body.id
$ovNew = Invoke-Api -Method GET -Path "/admin/Users/$newId/permission-overrides" -Token $adminToken
$patchNew = Invoke-Api -Method PATCH -Path "/admin/Users/$newId/permission-overrides" -Token $adminToken -Body @{
  expectedVersion = [int]$ovNew.Body.permissionsVersion
  changes = @(@{ permissionId = $userCreateId; mode = $ModeAllow })
}
Add-Result 'create-then-powers-step' $(if ($created.Ok -and $patchNew.Ok) { 'PASS' } else { 'FAIL' }) "create=$($created.Status) patch=$($patchNew.Status) id=$newId"

$ovProt = Invoke-Api -Method GET -Path '/admin/Users/1/permission-overrides' -Token $adminToken
Add-Result 'protected-account-flag' $(if ($ovProt.Ok -and ($ovProt.Body.isProtectedAccount -eq $true -or $ovProt.Body.canEditOverrides -eq $false)) { 'PASS' } else { 'FAIL' }) "protected=$($ovProt.Body.isProtectedAccount) canEdit=$($ovProt.Body.canEditOverrides)"

$outPath = 'C:\GitHub\accountant_qlt_system\docs\unified-admin-permissions\_phase02_live_results.json'
$Results | ConvertTo-Json -Depth 5 | Set-Content -Path $outPath -Encoding UTF8
$pass = @($Results | Where-Object Status -eq 'PASS').Count
$fail = @($Results | Where-Object Status -eq 'FAIL').Count
Write-Host "SUMMARY pass=$pass fail=$fail"
Write-Host "Wrote $outPath"
if ($fail -gt 0) { exit 2 }
