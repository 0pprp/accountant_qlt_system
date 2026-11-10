$ErrorActionPreference = 'Continue'
$ev = 'C:\GitHub\accountant_qlt_system\docs\sales-request-review\04h-live-evidence'
$shot = Join-Path $ev 'screenshots'
$logPath = Join-Path $ev 'flutter_complete_13_log.txt'
$lines = New-Object System.Collections.Generic.List[string]
function L([string]$m) { $x = "[{0}] {1}" -f (Get-Date -Format o), $m; $lines.Add($x); Write-Host $x }

function Dismiss {
  for ($i = 0; $i -lt 8; $i++) {
    $s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
    if ($s -match 'File chooser|Modal state') { return }
    if ($s -match 'button "حسناً" \[.*?ref=([^\]]+)\]') {
      playwright-cli -s=flutter click $Matches[1] 2>&1 | Out-Null; Start-Sleep 1; continue
    }
    break
  }
}

function ClickName([string]$name) {
  playwright-cli -s=flutter run-code "async (page) => { await page.getByRole('button', { name: $($name | ConvertTo-Json) }).click(); }" 2>&1 | Out-Null
}

function Body() {
  $r = playwright-cli -s=flutter eval "() => document.body.innerText || ''" 2>&1 | Out-String
  if ($r -match '(?s)### Result\s*"([\s\S]*?)"\s*\n### Ran') { return ($Matches[1] -replace '\\n', "`n") }
  return $r
}

playwright-cli -s=flutter close 2>&1 | Out-Null
playwright-cli -s=flutter open 'http://127.0.0.1:5180/' --headed 2>&1 | Out-Null
Start-Sleep 3
playwright-cli -s=flutter eval "() => { localStorage.clear(); sessionStorage.clear(); return 'ok'; }" 2>&1 | Out-Null
playwright-cli -s=flutter goto 'http://127.0.0.1:5180/' 2>&1 | Out-Null
Start-Sleep 3
playwright-cli -s=flutter eval "() => { const el=document.querySelector('flt-semantics-placeholder'); if(el) el.click(); return 'a11y'; }" 2>&1 | Out-Null
Start-Sleep 2
playwright-cli -s=flutter resize 390 844 2>&1 | Out-Null

playwright-cli -s=flutter fill 'اسم المستخدم' 'emp_a_04f' 2>&1 | Out-Null
playwright-cli -s=flutter fill 'كلمة المرور' 'EmpA123!' 2>&1 | Out-Null
ClickName 'تسجيل الدخول'
Start-Sleep 5
Dismiss
$t = Body
if ($t -match 'إنهاء الدوام') {
  playwright-cli -s=flutter eval "() => { history.back(); return 'b'; }" 2>&1 | Out-Null
  Start-Sleep 2
} elseif ($t -match 'بدء الدوام' -and $t -notmatch 'إنهاء') {
  ClickName 'بدء الدوام'; Start-Sleep 4; Dismiss
  playwright-cli -s=flutter eval "() => { history.back(); return 'b'; }" 2>&1 | Out-Null
  Start-Sleep 2
}
Dismiss
L ('home=' + (Body).Substring(0, [Math]::Min(280, (Body).Length)))
playwright-cli -s=flutter screenshot --filename (Join-Path $shot 'c13_home.png') 2>&1 | Out-Null

# Open inbox via bottom nav
ClickName 'طلبات البيع'
Start-Sleep 3
Dismiss
L ('inbox=' + (Body).Substring(0, [Math]::Min(400, (Body).Length)))
playwright-cli -s=flutter screenshot --filename (Join-Path $shot 'c13_inbox.png') 2>&1 | Out-Null

# Open #13
$s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
if ($s -match 'button "[^"]*#13[^"]*" \[ref=([^\]]+)\]') {
  playwright-cli -s=flutter click $Matches[1] 2>&1 | Out-Null
} elseif ($s -match 'button "[^"]*Flutter Complete[^"]*" \[ref=([^\]]+)\]') {
  playwright-cli -s=flutter click $Matches[1] 2>&1 | Out-Null
} else {
  L 'MISS open #13'
  $lines | Set-Content $logPath -Encoding UTF8
  exit 2
}
Start-Sleep 3
Dismiss
L ('detail=' + (Body).Substring(0, [Math]::Min(400, (Body).Length)))

# Prepare
ClickName 'تجهيز للبيع'
Start-Sleep 1
$s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
if ($s -match 'textbox .*\[ref=([^\]]+)\]') { playwright-cli -s=flutter fill $Matches[1] '04H Flutter prepare #13' 2>&1 | Out-Null }
ClickName 'تأكيد'
Start-Sleep 3
Dismiss
L ('after_prepare=' + (Body).Substring(0, [Math]::Min(300, (Body).Length)))

# Inspect
ClickName 'كشف'
Start-Sleep 2
Dismiss
# optional note dialog
$s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
if ($s -match 'button "تأكيد"') { ClickName 'تأكيد'; Start-Sleep 2; Dismiss }
L ('after_inspect=' + (Body).Substring(0, [Math]::Min(300, (Body).Length)))

# Start sale
ClickName 'بدء / استكمال البيع'
Start-Sleep 4
Dismiss
L ('wizard=' + (Body).Substring(0, [Math]::Min(300, (Body).Length)))
L ('href=' + ((playwright-cli -s=flutter eval "() => location.href" 2>&1 | Out-String)))

# Step1 fill
$s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
function Fill($n,$v){ if($script:s -match "textbox `"$([regex]::Escape($n))`"[^\n]*\[ref=([^\]]+)\]"){ playwright-cli -s=flutter fill $Matches[1] $v 2>&1|Out-Null } }
$script:s=$s
Fill 'اسم المحل *' 'محل 13'
Fill 'طبيعة العمل *' 'بقالة'
Fill 'تقدير قيمة البضاعة *' '400000'
Fill 'تقدير الوارد اليومي *' '40000'
Fill 'طول المحل *' '4'
Fill 'عرض المحل *' '3'
ClickName 'تحديد موقع المحل *'
Start-Sleep 2
Dismiss
# photo with upload handler race — click then upload
playwright-cli -s=flutter run-code "async (page) => { const p = page.waitForEvent('filechooser'); await page.getByRole('button', { name: 'صورة المحل *' }).click(); const fc = await p; await fc.setFiles(String.raw`C:\GitHub\accountant_qlt_system\docs\sales-request-review\04h-live-evidence\shop_stub.jpg`); }" 2>&1 | Out-Null
Start-Sleep 2
Dismiss
ClickName 'التالي'
Start-Sleep 2
Dismiss

# Step2 customer — Mandob may have real list now
$s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
$script:s=$s
if ($s -match 'button "اختر القائمة') { ClickName 'اختر القائمة/المندوب من قوائم المحافظة'; Start-Sleep 1
  $s2 = playwright-cli -s=flutter snapshot 2>&1 | Out-String
  if ($s2 -match 'menuitem "[^"]+" \[ref=([^\]]+)\]') { playwright-cli -s=flutter click $Matches[1] 2>&1 | Out-Null }
}
$s = playwright-cli -s=flutter snapshot 2>&1 | Out-String
$script:s=$s
Fill 'الهاتف *' '07704445566'
Fill 'المحافظة *' 'بغداد'
Fill 'البطاقة الوطنية *' 'N998877'
Fill 'العنوان *' 'الكرادة'
Fill 'أقرب نقطة دالة *' 'ساحة'
Fill 'اسم المختار *' 'أبو محمد'
ClickName 'التالي'
Start-Sleep 2
Dismiss
ClickName 'التالي' # products
Start-Sleep 2
Dismiss
ClickName 'اعتماد الافتراضي من المنتجات'
Start-Sleep 1
ClickName 'التالي' # prices -> review
Start-Sleep 2
Dismiss
L ('review=' + (Body).Substring(0, [Math]::Min(500, (Body).Length)))
playwright-cli -s=flutter screenshot --filename (Join-Path $shot 'c13_review.png') 2>&1 | Out-Null
ClickName 'تم البيع'
Start-Sleep 8
Dismiss
L ('after_sold=' + (Body).Substring(0, [Math]::Min(500, (Body).Length)))
playwright-cli -s=flutter screenshot --filename (Join-Path $shot 'c13_after_sold.png') 2>&1 | Out-Null
$lines | Set-Content $logPath -Encoding UTF8
L 'wrote log'
