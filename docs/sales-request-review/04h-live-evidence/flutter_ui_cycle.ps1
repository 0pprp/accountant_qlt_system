$ErrorActionPreference = 'Stop'
$ev = 'C:\GitHub\accountant_qlt_system\docs\sales-request-review\04h-live-evidence'
$shots = Join-Path $ev 'screenshots'
New-Item -ItemType Directory -Force -Path $shots | Out-Null
$log = New-Object System.Collections.Generic.List[string]
function L($m) { $log.Add(("[{0}] {1}" -f (Get-Date -Format o), $m)); Write-Host $m }

function Snap([string]$name) {
  playwright-cli screenshot --filename (Join-Path $shots $name) 2>&1 | Out-Null
}

function Dismiss-Ok {
  for ($i = 0; $i -lt 6; $i++) {
    $s = playwright-cli snapshot 2>&1 | Out-String
    if ($s -match 'button "حسناً" \[.*?ref=([^\]]+)\]') {
      playwright-cli click $Matches[1] 2>&1 | Out-Null
      Start-Sleep 1
    } else { break }
  }
}

function Click-ByLabel([string]$label) {
  $s = playwright-cli snapshot 2>&1 | Out-String
  if ($s -match "button `"$([regex]::Escape($label))`" \[.*?ref=([^\]]+)\]") {
    playwright-cli click $Matches[1] 2>&1 | Out-Null
    return $true
  }
  return $false
}

function TextContains([string]$needle) {
  $t = playwright-cli eval "() => document.body.innerText || ''" 2>&1 | Out-String
  return $t.Contains($needle)
}

playwright-cli close 2>&1 | Out-Null
playwright-cli open 'http://127.0.0.1:5180/' --headed 2>&1 | Out-Null
Start-Sleep 2
playwright-cli eval "() => { localStorage.clear(); sessionStorage.clear(); return 'ok'; }" 2>&1 | Out-Null
playwright-cli goto 'http://127.0.0.1:5180/' 2>&1 | Out-Null
Start-Sleep 3
playwright-cli eval "() => { const el=document.querySelector('flt-semantics-placeholder'); if(el){el.click();return 'a11y';} return 'none'; }" 2>&1 | Out-Null
Start-Sleep 2
playwright-cli resize 390 844 2>&1 | Out-Null

# Login
$s = playwright-cli snapshot 2>&1 | Out-String
if ($s -notmatch 'textbox "اسم المستخدم" \[ref=([^\]]+)\]') { throw 'login form missing' }
$userRef = $Matches[1]
$null = $s -match 'textbox "كلمة المرور" \[ref=([^\]]+)\]'; $passRef = $Matches[1]
$null = $s -match 'button "تسجيل الدخول" \[ref=([^\]]+)\]'; $loginRef = $Matches[1]
playwright-cli click $userRef 2>&1 | Out-Null
playwright-cli type 'emp_a_04f' 2>&1 | Out-Null
playwright-cli click $passRef 2>&1 | Out-Null
playwright-cli type 'EmpA123!' 2>&1 | Out-Null
playwright-cli click $loginRef 2>&1 | Out-Null
Start-Sleep 3
Dismiss-Ok
L 'logged in as emp_a_04f'
Snap 'ui_01_after_login.png'

# Duty
if (Click-ByLabel 'بدء الدوام') {
  Start-Sleep 2
  Dismiss-Ok
  L 'duty started (GPS stub on web)'
} else {
  L 'duty already active or button absent'
}
Snap 'ui_02_home.png'
Start-Sleep 3
Dismiss-Ok

# Verify bins loaded (counts) — look for status labels
$text = playwright-cli eval "() => document.body.innerText || ''" 2>&1 | Out-String
L ("home_text_snippet=" + $text.Substring(0, [Math]::Min(400, $text.Length)))
$binsOk = ($text -match 'جاهز' -or $text -match 'تم الكشف' -or $text -match 'معلقة' -or $text -match 'حالات الطلبات')
L ("bins_section_visible=$binsOk")

# Open bottom-nav طلبات البيع → SalesRequestsBinPage
if (-not (Click-ByLabel 'طلبات البيع')) { throw 'bottom nav طلبات البيع missing' }
Start-Sleep 3
Dismiss-Ok
Snap 'ui_03_sales_requests_inbox.png'
$text = playwright-cli eval "() => document.body.innerText || ''" 2>&1 | Out-String
L ("inbox_snippet=" + $text.Substring(0, [Math]::Min(500, $text.Length)))
if ($text -match 'سجلات البيع') { throw 'NAV REGRESSION: opened Orders/سجلات البيع instead of SalesRequests' }
if ($text -match 'إعادة المحاولة' -or $text -match 'حدث خطأ') {
  L 'network error on inbox — clicking retry'
  Click-ByLabel 'إعادة المحاولة' | Out-Null
  Start-Sleep 3
  Dismiss-Ok
  $text = playwright-cli eval "() => document.body.innerText || ''" 2>&1 | Out-String
  L ("inbox_after_retry=" + $text.Substring(0, [Math]::Min(500, $text.Length)))
}

# Open request #9
$opened = $false
$s = playwright-cli snapshot 2>&1 | Out-String
# Prefer list tile containing #9
if ($text -match '04H' -or $text -match '#9') {
  # Click via evaluate on semantics or text
  $r = playwright-cli eval "() => { const nodes=[...document.querySelectorAll('[role=button],flt-semantics')]; const hit=nodes.find(n=>(n.getAttribute('aria-label')||n.innerText||'').includes('04H')|| (n.getAttribute('aria-label')||'').includes('#9')); if(hit){hit.click();return 'clicked'} return 'miss' }" 2>&1 | Out-String
  L "open_card=$r"
  Start-Sleep 2
  # Fallback: tap first list-looking button after title
  $s2 = playwright-cli snapshot 2>&1 | Out-String
  if ($s2 -match '04H Web Cycle') {
    # find any clickable near customer name — use find + click pattern from snapshot lines with generic
    playwright-cli find '04H' 2>&1 | Out-Null
  }
  # Use playwright click on text
  try {
    playwright-cli eval "() => { const all=[...document.querySelectorAll('*')]; const el=all.find(e=> (e.innerText||'').includes('04H Web Cycle') && e.getAttribute('role')); if(el){el.click();return 'ok'} const t=all.find(e=>(e.innerText||'').trim().startsWith('04H')); if(t){t.click(); return 'text'} return 'no' }" 2>&1 | Out-Null
  } catch {}
  Start-Sleep 2
  $opened = TextContains '#9' -or TextContains 'رقم الطلب' -or TextContains 'تجهيز'
  L "detail_opened_guess=$opened"
}
Snap 'ui_04_request_detail.png'

# If still on list, try snapshot refs for ListTile-ish buttons
$s = playwright-cli snapshot 2>&1 | Out-String
if ($s -notmatch 'تجهيز|كشف|إكمال|بدء البيع|رقم الطلب') {
  # Click first card-like node under طلبات البيع group
  $lines = $s -split "`n" | Where-Object { $_ -match '04H|مسند|#' }
  L ("candidate_lines=" + ($lines | Select-Object -First 8 | Out-String))
}

# Actions: prepare — look for button
$acted = $false
foreach ($label in @('تجهيز','تجهيز للبيع','اطلاع')) {
  if (Click-ByLabel $label) { $acted = $true; L "clicked $label"; break }
}
Start-Sleep 1
# Note dialog — type and confirm
$s = playwright-cli snapshot 2>&1 | Out-String
if ($s -match 'textbox' -or $s -match 'اكتب') {
  if ($s -match 'textbox[^\n]*\[ref=([^\]]+)\]') {
    playwright-cli click $Matches[1] 2>&1 | Out-Null
    playwright-cli type '04H UI prepare note' 2>&1 | Out-Null
  }
  Click-ByLabel 'تأكيد' | Out-Null
  Start-Sleep 2
  Dismiss-Ok
  L 'prepare submitted via UI'
}
Snap 'ui_05_after_prepare.png'

# Inspect
foreach ($label in @('كشف','تم الكشف','إجراء الكشف')) {
  if (Click-ByLabel $label) { L "clicked $label"; break }
}
# Also try from snapshot for common action buttons
$s = playwright-cli snapshot 2>&1 | Out-String
L ("detail_actions_snapshot=" + (($s -split "`n" | Where-Object { $_ -match 'button' } | Select-Object -First 20) -join ' | '))
Start-Sleep 2
Dismiss-Ok
Snap 'ui_06_after_inspect.png'

# Start sale / ensure draft
foreach ($label in @('بدء البيع','متابعة البيع','إتمام البيع','ضمان المسودة')) {
  if (Click-ByLabel $label) { L "clicked $label"; Start-Sleep 2; break }
}
Dismiss-Ok
Snap 'ui_07_sale_flow.png'

$text = playwright-cli eval "() => document.body.innerText || ''" 2>&1 | Out-String
L ("final_ui_text=" + $text.Substring(0, [Math]::Min(800, $text.Length)))
$log | Set-Content (Join-Path $ev 'flutter_ui_cycle_log.txt') -Encoding UTF8
L 'wrote flutter_ui_cycle_log.txt'
