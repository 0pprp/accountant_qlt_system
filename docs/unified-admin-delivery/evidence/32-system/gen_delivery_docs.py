# -*- coding: utf-8 -*-
from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from pathlib import Path

base = Path(r"C:\GitHub\accountant_qlt_system\docs\unified-admin-delivery")

wb = Workbook()
ws = wb.active
ws.title = "Functions"
headers = [
    "الوظيفة",
    "Endpoint",
    "صلاحية",
    "تطبيق مستهلك",
    "اختبار",
    "نتيجة",
    "دليل",
    "مانع نشر إن وجد",
    "مستوى إثبات",
]
ws.append(headers)
for c in ws[1]:
    c.font = Font(bold=True)
    c.fill = PatternFill("solid", fgColor="D9E2F3")

rows = [
    ["تنازلات Create", "POST /admin/Waivers", "Waiver.Create", "لوحة/BM", "API-live create bm_iso", "PASS", "waiver-financial", "—", "API-live"],
    ["تنازلات Reject", "POST /admin/Waivers/{id}/reject", "Waiver.Decide", "لوحة Admin", "Pending→Rejected", "PASS", "waiver-financial.json", "SelfDecide لصاحب الطلب", "API-live"],
    ["تنازلات Approve", "POST .../approve", "Waiver.Decide", "لوحة", "→AwaitingAccountant", "PASS", "waiver-financial.json", "—", "API-live"],
    ["تنازلات Apply", "POST .../apply", "Waiver.Apply", "لوحة Admin", "SellAmount −50 SQL", "PASS", "waiver-financial.json", "بوابة مالية", "API-live"],
    ["تنازلات Reapply", "POST .../apply", "Waiver.Apply", "لوحة", "لا خصم ثانٍ", "PASS idempotent", "waiver-financial.json", "—", "API-live"],
    ["ملاحظات تطبيقات", "POST /app/AppNotes", "AppNote.Create", "مندوب/متابع", "إنشاء+list إدارة", "PASS", "live-api-32b / cycles", "Device Missing", "API-live"],
    ["مهام إدارية", "GET /admin/ManagementTasks", "ManagementTask.*", "لوحة", "Inbox", "PASS", "live-api-32-cycles", "concurrency مستقل Missing", "API-live"],
    ["مهام لجنة", "GET /admin/CommitteeTasks", "Committee.*", "لوحة/تطبيق لجنة", "list", "PASS list", "live-api-32-cycles", "Device Missing", "API-live"],
    ["LegalFollowups", "GET /admin/LegalFollowups", "Legal.*", "لوحة", "list", "PASS", "live-api-32-cycles", "intake Browser", "API-live"],
    ["PriceBoard", "GET /admin/PriceBoard", "PriceBoard.*", "لوحة/تطبيق", "list", "PASS empty", "live-api-32-cycles", "إنشاء/نشر", "API-live"],
    ["Announcements", "GET /admin/Announcements", "Announcement.*", "لوحة/تطبيقات", "list", "PASS", "live-api-32", "create عقد", "API-live"],
    ["TutorialVideos", "GET /admin/TutorialVideos", "TutorialVideo.*", "لوحة/تطبيقات", "list", "PASS", "live-api-32-cycles", "Device رابط", "API-live"],
    ["BranchNewSales", "GET /admin/BranchNewSales", "…", "لوحة فرع/مفوض", "list/mark-read", "PASS", "31 + 32b", "—", "API-live+Browser-31"],
    ["BranchCustomers", "GET .../search", "Customer.Read", "لوحة", "search", "PASS", "32b", "—", "API-live"],
    ["BranchPayments", "GET /admin/BranchPayments", "…", "لوحة", "list listId", "PASS", "32b", "—", "API-live"],
    ["BranchVisits", "open/depart/ratings", "BranchVisit.*", "لوحة", "دورة 31", "PASS 31", "evidence/31", "—", "API-live+Browser-31"],
    ["WrongSales لوحة", "/admin/WrongSales", "WrongSale.*", "لوحة", "دورة 31", "PASS 31", "evidence/31", "—", "API-live+Browser-31"],
    ["WrongSale أول تحصيل", "POST /app/WrongSales/first-collection-eval", "InstallmentPayment.Create", "delegate_app", "API/تكليف", "400 بدون تكليف كامل؛ APK يُبنى", "32 + APK hash", "Device-verified Missing", "Build-tested"],
    ["SalesRequests", "GET Unassigned", "SalesRequest.*", "لوحة+3 تطبيقات", "list", "PASS total=6", "live-api-32", "دورة UI كاملة", "API-live"],
    ["Authz follower Waivers", "GET /admin/Waivers", "—", "—", "403", "PASS deny", "live-api-32-cycles", "—", "API-live"],
    ["Authz no token", "GET /admin/Waivers", "—", "—", "401", "PASS", "live-api-32", "—", "API-live"],
    ["Unit tests", "Application.Tests", "—", "—", "dotnet test Release", "412 passed", "unit-tests-release.txt", "—", "Unit-tested"],
    ["Panel build", "vite", "—", "panel", "production build", "PASS", "vite-build.txt + hashes", "—", "Build"],
    ["DB fresh migrate", "EF migrations", "—", "—", "qalaat_32_fresh", "PASS 46", "db-status-32-fresh.txt", "seed on boot", "DB"],
    ["DB upgrade iso", "EF", "—", "—", "latest Branch31", "PASS", "db-status-22-23.txt", "—", "DB"],
    ["delegate APK", "flutter build apk", "—", "delegate_app", "debug APK", "PASS", "delegate-apk-hash.txt", "Device Missing", "Build-tested"],
    ["sales APK", "flutter build apk", "—", "sales_employee_app", "debug APK", "PASS", "sales-apk-hash.txt", "Device Missing", "Build-tested"],
]
for r in rows:
    ws.append(r)

ws2 = wb.create_sheet("Readiness")
ws2.append(["وحدة", "قرار", "شرط/سبب"])
for r in [
    ["الإصدار الكلي", "جاهزة بشروط", "لا إنتاج؛ Device ناقص؛ tag نظيف مُستحسن"],
    ["التنازلات المالية", "جاهزة ضمن نطاق API", "SQL SellAmount مثبت"],
    ["ملاحظات التطبيقات", "جاهزة ضمن نطاق API", "Device ناقص"],
    ["31 فرع/مفوض", "جاهزة ضمن نطاق مختبر", "Browser 31"],
    ["Flutter ميداني", "جاهزة بشروط Build", "ليس Device-verified"],
    ["GPS خلفية", "غير جاهزة", "Device"],
]:
    ws2.append(r)

ws3 = wb.create_sheet("Artifacts")
ws3.append(["ملف", "SHA256"])
for r in [
    ["Api.dll", "ED9A9C0CD542841ED77037BED14738DDB7E3E2354DBBD8403E0484291056C167"],
    ["Application.dll", "DC94CE412FA62B19CED7A63903AE14D0FDB122EC1A392963BAFF577021ABA767"],
    ["panel index-CQLR3Srj.js", "CB836CDFC8CF91D0194832BA85D3FBA3D06C123D2C3A1C3F423311DBFEE9C9DF"],
    ["delegate APK", "575570D124C236DCC1C5386EB03EB0B34D110067C9111B1AAA8F2EB92D85589C"],
    ["sales APK", "28C46E0CA3EE3CB155B6BE77959C33CC309D3D90FC3DF05D315FF30FD979593B"],
    ["HEAD commit", "650e3a16f66668378c3273d74c9cb7198fdc8e4d"],
]:
    ws3.append(r)

xlsx = base / "Unified_Admin_Delivery_32.xlsx"
wb.save(xlsx)
print("xlsx", xlsx)

doc = Document()
doc.add_heading("Unified Admin Delivery 32 — Architecture", level=0)
p = doc.add_paragraph("مرشح إصدار مختبر على iso — دون نشر إنتاج.")
p.alignment = WD_ALIGN_PARAGRAPH.RIGHT

doc.add_heading("1. البنية النهائية", level=1)
doc.add_paragraph(
    "API (.NET 9 CQRS) + Panel Vue RTL + تطبيقات Flutter (مندوب، موظف مبيعات، لجنة، app-dev). "
    "واجهات مدير الفرع والمفوض ويب عبر اللوحة الموحدة. Postgres عبر EF Core migrations "
    "(46 حتى Branch31VisitMembersAttachmentsAndWrongSaleSource)."
)

doc.add_heading("2. الملفات ومسؤولياتها", level=1)
for t in [
    "api-main: عقود HTTP، صلاحيات، Waivers/Visits/WrongSales/AppNotes/…",
    "panel-admin-main: الآباء الأربعة، صفحات الفرع/المفوض، الرقابة، المبيعات.",
    "delegate_app: تحصيل، أول تقييم WrongSale، ملاحظات إدارة، إعلانات.",
    "docs/unified-admin-delivery: تغطية الدفعات + evidence + runbooks.",
]:
    doc.add_paragraph(t, style="List Bullet")

doc.add_heading("3. دورات الأعمال", level=1)
doc.add_paragraph(
    "طلب البيع، القانونية، المهام، الزيارات، WrongSale، المحتوى، وبوابة CustomerWaiverRequest "
    "(Pending→Rejected و Pending→AwaitingAccountant→Apply→Approved). أثبتت 32 أن Apply يخصم "
    "Order.SellAmount بعد الموافقة فقط، وأن إعادة Apply لا تخصم مرة ثانية."
)

doc.add_heading("4. الترقية والنشر والرجوع", level=1)
doc.add_paragraph(
    "انظر Deployment_Runbook_32 و Rollback_Runbook_32. مسار fresh: qalaat_32_fresh. "
    "مسار ترقية: qalaat_22_23_iso. رجوع الصورة لا يصلح مخططًا أحدث؛ الرجوع الآمن للبيانات عبر backup معزول."
)

doc.add_heading("5. الحدود المعروفة", level=1)
for t in [
    "Device-verified Missing لجميع المسارات الميدانية في هذه الجلسة.",
    "الإصدار من working tree فوق 650e3a16 وليس tag نظيف.",
    "حسابات iso: admin, follower_iso, mandob_iso, bm_iso فقط.",
    "Realtime/أداء على بيانات صغيرة — لا ادّعاء إنتاج.",
    "إنشاء إعلان يحتاج تثبيت عقد الجسم؛ list فقط مثبت.",
    "لا تُعلن النظام كاملاً مع نواقص جوهرية Missing.",
]:
    doc.add_paragraph(t, style="List Bullet")

doc.add_heading("6. قرار الجاهزية", level=1)
doc.add_paragraph(
    "جاهزة بشروط واضحة للنشر التدريجي غير الإنتاجي؛ غير جاهزة لإنتاج عام دون Device وحسابات أدوار وtag نظيف."
)

docx_path = base / "Unified_Admin_Delivery_32_Architecture.docx"
doc.save(docx_path)
print("docx", docx_path)
