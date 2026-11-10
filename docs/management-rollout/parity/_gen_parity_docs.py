# -*- coding: utf-8 -*-
from pathlib import Path

from docx import Document
from openpyxl import Workbook

ROOT = Path(r"C:\GitHub\accountant_qlt_system\docs\management-rollout\parity")

# —— Excel ——
wb = Workbook()
ws = wb.active
ws.title = "ParityMatrix"
ws.append(
    [
        "Role",
        "Old tab",
        "Old screen/rule",
        "Source evidence",
        "New counterpart",
        "Verdict",
        "Correction",
    ]
)
rows = [
    (
        "المدير+المراقب",
        "متابعة المندوبين",
        "محافظة→قوائم مندوبين نشطة≤40ي→ملخص→جدول؛ تقييم عامل لوحة",
        "FE_Company/director-delegate-followup.vue; DirectorDelegateFollowupService",
        "لا مسار. SalesFollowup=موظفو مبيعات تحت تسميات مضللة",
        "مختلف/غير موجود",
        "لوحة مستقلة؛ الإبقاء على SalesFollowup باسمه الصحيح فقط",
    ),
    (
        "المدير+المراقب",
        "عمل المتابعين",
        "تبويبات: مصادقة/متدنية/ملاحظات/قديمة/ملاحظات مندوبين/جباية/خرائط",
        "director-follower-work.vue; DirectorFollowerOps*",
        "لا مقابل. ManagementTasks≠عمل متابعين",
        "غير موجود",
        "برومبتات فرعية لاحقاً؛ لا ربط بالمهام",
    ),
    (
        "المدير فقط",
        "الملخص",
        "مصفوفة كلي/فعلي حسب فرع ويوم",
        "director-summary.vue; DirectorSummaryService",
        "Core Dashboard عام",
        "مختلف/غير موجود",
        "شاشة ملخص مستقلة؛ لا استبدال Dashboard",
    ),
    (
        "مدير فرع",
        "مهامي…سجل القرارات (10)",
        "Drawer BM",
        "branch_manager_application/ui/home_shell.dart",
        "Management branch جزئي",
        "ناقص",
        "الحفاظ على الجاهز؛ استكمال لاحق",
    ),
    (
        "مفوض",
        "الرئيسية…الإعدادات (10)",
        "HomeShell DM",
        "delegated_manager_application/.../home_shell.dart",
        "Management delegate جزئي",
        "ناقص",
        "الحفاظ على الجاهز؛ استكمال لاحق",
    ),
    (
        "اللوحة الجديدة",
        "تقييم موظفي المبيعات",
        "—",
        "sections.ts salesEmployeeRating",
        "SalesFollowupPage (نفس متابعة المبيعات)",
        "ربط خاطئ بالتسمية",
        "فك التباس التسمية؛ ليست متابعة مندوبين",
    ),
    (
        "اللوحة الجديدة",
        "المهام المسندة",
        "Assigned tasks admin",
        "ManagementTasks module",
        "يقابل مهامي BM/DM",
        "مطابق جزئياً لـمهامي / مختلف عن عمل المتابعين",
        "الإبقاء؛ لا تسمية عمل متابعين",
    ),
]
for r in rows:
    ws.append(list(r))

w2 = wb.create_sheet("OldDirectorMenu")
w2.append(["Order", "Title", "Route/Query", "Roles", "Notes"])
for r in [
    (1, "الملخص", "director-summary", "المدير", "أولوية 3"),
    (2, "متابعة المندوبين", "director-delegate-followup?city=", "المدير+المراقب", "أولوية 1 — برومبت 01"),
    (3, "متابعة المبيعات", "(مسار منفصل)", "المدير+المراقب", "≠ متابعة المندوبين"),
    (4, "عمل المتابعين / مصادقة البيع", "director-follower-work?tab=sales", "المدير+المراقب", "أولوية 2"),
    (5, "عمل المتابعين / المتدنية", "tab=declining", "المدير+المراقب", ""),
    (6, "عمل المتابعين / الملاحظات", "tab=notes", "المدير+المراقب", ""),
    (7, "عمل المتابعين / الزبائن القديمة", "tab=oldCustomers", "المدير+المراقب", ""),
    (8, "عمل المتابعين / ملاحظات المندوبين", "tab=delegateNotes", "المدير+المراقب", ""),
    (9, "عمل المتابعين / الجباية", "tab=collection", "المدير+المراقب", ""),
]:
    w2.append(list(r))

w3 = wb.create_sheet("ReviewMode")
w3.append(["Item", "Value"])
w3.append(["Review type", "Source-only — FE_Company/BM/DM not run live"])
w3.append(["No DB changes", "Yes"])
w3.append(["No publish", "Yes"])
w3.append(["No feature implementation in this audit", "Yes"])
w3.append(["First executable prompt", "prompts/01-director-delegate-followup.md"])

wb.save(ROOT / "WorkflowParity.xlsx")
print("XLSX_OK")

# —— Word ——
doc = Document()
doc.add_heading("معمارية تكافؤ مسار الإدارة — تدقيق", level=1)
doc.add_paragraph(
    "مراجعة مصدرية 2026-11-08. لا تشغيل حي للمرجع. لا نشر ولا تغيير قاعدة. "
    "التغييرات الحالية في المستودع محفوظة (انظر GitSnapshot_BeforeAudit.md)."
)

doc.add_heading("خريطة المسؤوليات", level=2)
for line in [
    "NavigationParity.md — تبويبات BM/DM/FE_Company واللوحة الجديدة",
    "GapsAndCorrections.md — مصفوفة مطابق/ناقص/مختلف/غير موجود",
    "WorkflowParity.xlsx — نفس المصفوفة جداول",
    "prompts/01-director-delegate-followup.md — أول برومبت تنفيذي لمتابعة المندوبين",
]:
    doc.add_paragraph(line, style="List Bullet")

doc.add_heading("اكتشاف جوهري", level=2)
doc.add_paragraph(
    "المجالات الثلاثة ذات الأولوية (متابعة المندوبين، عمل المتابعين، الملخص) "
    "موجودة في FE_Company لأدوار المدير/المراقب، وليست تبويبات في تطبيقات Flutter لمدير الفرع أو المفوض."
)
doc.add_paragraph(
    "اللوحة الجديدة ربطت SalesFollowup (موظفو مبيعات) وManagementTasks (مهام إدارية) "
    "بتسميات/أماكن قد تُفهم خطأً كبديل لهذه المجالات — التصحيح = فك الربط الخاطئ دون حذف الوظائف المستقلة."
)

doc.add_heading("مسارات الملفات المرجعية للمجال الأول", level=2)
for f in [
    "FE_Company/src/pages/director-delegate-followup.vue",
    "BE_Company/Sales/Services/DirectorDelegateFollowupService.cs",
    "BE_Company/Sales/Controllers/DirectorDelegateFollowupController.cs",
    "BE_SalesEmployee/Controllers/DirectorController.cs (delegate-followup)",
]:
    doc.add_paragraph(f, style="List Bullet")

doc.add_heading("الخطوة التالية", level=2)
doc.add_paragraph(
    "اعتماد المقارنة من المستخدم، ثم تنفيذ برومبت 01 فقط بعد الموافقة الصريحة. "
    "برومبتات عمل المتابعين والملخص تُكتب لاحقاً بنفس المنهج."
)

doc.save(ROOT / "WorkflowArchitecture.docx")
print("DOCX_OK")
print("FILES", sorted(p.name for p in ROOT.iterdir() if p.is_file() or p.is_dir()))
