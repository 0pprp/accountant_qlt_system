$ErrorActionPreference = 'Continue'
$ev = 'C:\GitHub\accountant_qlt_system\docs\sales-request-review\04h-live-evidence'
$shot = Join-Path $ev 'screenshots'
$log = New-Object System.Collections.Generic.List[string]
function L([string]$m) { $line = "[{0}] {1}" -f (Get-Date -Format o), $m; $log.Add($line); Write-Host $line }

function Dismiss([string]$session = 'flutter') {
  for ($i = 0; $i -lt 8; $i++) {
    $s = playwright-cli -s=$session snapshot 2>&1 | Out-String
    if ($s -match 'button "حسناً" \[.*?ref=([^\]]+)\]') {
      playwright-cli -s=$session click $Matches[1] 2>&1 | Out-Null
      Start-Sleep 1
      continue
    }
    if ($s -match 'button "إغلاق" \[.*?ref=([^\]]+)\]') {
      playwright-cli -s=$session click $Matches[1] 2>&1 | Out-Null
      Start-Sleep 1
      continue
    }
    break
  }
}

function Snap([string]$name) {
  playwright-cli -s=flutter screenshot --filename (Join-Path $shot $name) 2>&1 | Out-Null
}

function BodyText() {
  $r = playwright-cli -s=flutter eval "() => document.body.innerText || ''" 2>&1 | Out-String
  if ($r -match '(?s)### Result\s*"([\s\S]*?)"\s*\n### Ran') { return $Matches[1] -replace '\\n', "`n" }
  return $r
}

function ClickLabel([string]$label) {
  $s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
  if ($s -match ("button `"$([regex]::Escape($label))`" \[ref=([^\]]+)\]")) {
    playwright-cli -s=flutter click $Matches[1] 2>&1 | Out-Null
    return $true
  }
  return $false
}

# --- Flutter session ---
playwright-cli -s=flutter close 2>&1 | Out-Null
playwright-cli -s=flutter open 'http://127.0.0.1:5180/' --headed 2>&1 | Out-Null
Start-Sleep 3
playwright-cli -s=flutter eval "() => { localStorage.clear(); sessionStorage.clear(); return 'cleared'; }" 2>&1 | Out-Null
playwright-cli -s=flutter goto 'http://127.0.0.1:5180/' 2>&1 | Out-Null
Start-Sleep 3
playwright-cli -s=flutter eval "() => { const el=document.querySelector('flt-semantics-placeholder'); if(el){el.click(); return 'a11y';} return 'none'; }" 2>&1 | Out-Null
Start-Sleep 2
playwright-cli -s=flutter resize 390 844 2>&1 | Out-Null

# Login
playwright-cli -s=flutter fill 'اسم المستخدم' 'emp_a_04f' 2>&1 | Out-Null
playwright-cli -s=flutter fill 'كلمة المرور' 'EmpA123!' 2>&1 | Out-Null
ClickLabel 'تسجيل الدخول' | Out-Null
Start-Sleep 5
Dismiss
L ('after_login=' + ((BodyText).Substring(0, [Math]::Min(200, (BodyText).Length))))

# Leave mandatory shift if active (canPop when active)
$text = BodyText
if ($text -match 'دوام مفتوح|بدء الدوام إلزامي') {
  if ($text -match 'بدء الدوام' -and $text -notmatch 'إنهاء الدوام') {
    ClickLabel 'بدء الدوام' | Out-Null
    Start-Sleep 4
    Dismiss
  }
  playwright-cli -s=flutter press Escape 2>&1 | Out-Null
  Start-Sleep 1
  # If still on shift with active duty, browser back often pops when canPop=true
  playwright-cli -s=flutter eval "() => { history.back(); return location.href; }" 2>&1 | Out-Null
  Start-Sleep 2
}
Dismiss
Snap 'cycle_home.png'
$home = BodyText
L ('home=' + $home.Substring(0, [Math]::Min(400, $home.Length)))

# Prefer inspected bin card, else bottom-nav inbox then detail via search
$opened = $false
if (ClickLabel '1 تم الكشف') { $opened = $true; L 'clicked bin 1 تم الكشف' }
elseif (ClickLabel 'تم الكشف') { $opened = $true; L 'clicked تم الكشف' }
Start-Sleep 3
Dismiss
$bin = BodyText
L ('bin_or_page=' + $bin.Substring(0, [Math]::Min(300, $bin.Length)))
Snap 'cycle_bin.png'

# If wrongly on Orders, go back and use bottom nav + open via inbox won't have inspected.
# Use hash deep-nav isn't available; open from bottom nav then we need inspected list.
if ($bin -match 'سجلات البيع') {
  L 'WARN: home bin opened Orders — navigating back and using SalesRequests bottom nav + detail push via bin list API path'
  ClickLabel 'رجوع' | Out-Null
  Start-Sleep 1
  ClickLabel 'طلبات البيع' | Out-Null
  Start-Sleep 2
}

# Open #9 if visible
$s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
if ($s -match 'button "[^"]*#9[^"]*" \[ref=([^\]]+)\]') {
  playwright-cli -s=flutter click $Matches[1] 2>&1 | Out-Null
  L 'opened #9 from list'
  Start-Sleep 3
  Dismiss
} elseif ($home -match '1 تم الكشف' -or $bin -match 'تم الكشف') {
  # try again from home inspected
  ClickLabel 'رجوع' | Out-Null
  Start-Sleep 1
  if (ClickLabel '1 تم الكشف') { Start-Sleep 3; Dismiss }
  $s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
  if ($s -match 'button "[^"]*#9[^"]*" \[ref=([^\]]+)\]') {
    playwright-cli -s=flutter click $Matches[1] 2>&1 | Out-Null
    L 'opened #9 after retry'
    Start-Sleep 3
    Dismiss
  } elseif ($s -match 'button "[^"]*04H[^"]*" \[ref=([^\]]+)\]') {
    playwright-cli -s=flutter click $Matches[1] 2>&1 | Out-Null
    L 'opened 04H card'
    Start-Sleep 3
    Dismiss
  }
}

$detail = BodyText
L ('detail=' + $detail.Substring(0, [Math]::Min(500, $detail.Length)))
Snap 'cycle_detail.png'

# Start / continue sale
foreach ($lab in @('بدء / استكمال البيع', 'بدء البيع', 'متابعة البيع')) {
  if (ClickLabel $lab) { L "clicked $lab"; break }
}
Start-Sleep 4
Dismiss
# If file chooser appears, upload stub
$probe = playwright-cli -s=flutter snapshot 2>&1 | Out-String
if ($probe -match 'File chooser|modal state') {
  playwright-cli -s=flutter upload (Join-Path $ev 'shop_stub.jpg') 2>&1 | Out-Null
  L 'uploaded shop_stub for file chooser'
  Start-Sleep 2
}
$sale = BodyText
L ('after_start_sale=' + $sale.Substring(0, [Math]::Min(500, $sale.Length)))
Snap 'cycle_start_sale.png'
L ('href=' + ((playwright-cli -s=flutter eval "() => location.href" 2>&1 | Out-String)))

$log | Set-Content (Join-Path $ev 'flutter_ui_cycle_resume_log.txt') -Encoding UTF8
L 'done resume script'
