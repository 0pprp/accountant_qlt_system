# -*- coding: utf-8 -*-
"""مولّد توثيق مرحلة 02 — صلاحيات المستخدم. التشغيل: python generate_docs.py"""
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from openpyxl.utils import get_column_letter
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Pt

ROOT = Path(__file__).resolve().parent
HEADER_FILL = PatternFill("solid", fgColor="006A64")
HEADER_FONT = Font(color="FFFFFF", bold=True, name="Arial")
BODY_FONT = Font(name="Arial", size=11)
THIN = Border(
    left=Side(style="thin"),
    right=Side(style="thin"),
    top=Side(style="thin"),
    bottom=Side(style="thin"),
)

ENDPOINTS = [
    ("GET", "/api/v1/admin/Users/{id}/permission-overrides", "User.ManagePermissions", "منفّذ + مختبر حي"),
    ("PATCH", "/api/v1/admin/Users/{id}/permission-overrides", "User.ManagePermissions + expectedVersion", "منفّذ؛ 409 حي"),
    ("PUT", "/api/v1/admin/Users/{id}/permissions", "ManagePermissions + expectedVersion إلزامي", "منفّذ؛ لا يمسح المنع"),
    ("PUT", "/api/v1/admin/Users/{id}", "User.Update؛ تغيير دور/فروع يتطلب سوبر أدمن", "منفّذ + مختبر حي"),
    ("POST", "/api/v1/admin/Users", "User.Create؛ تعيين دور/فروع سوبر أدمن فقط", "منفّذ + مختبر حي"),
]

# وحدة / محاكاة — منفصلة عن الربط الحي
ACCEPTANCE_UNIT = [
    ("AC-calc", "Inherit/Allow/Deny", "passed-unit", "EffectivePermissionCalculatorTests"),
    ("AC-concurrency-token", "تعارض PermissionsVersion SQLite", "passed-unit", "PermissionsVersionConcurrencyTests"),
    ("AC-handlers", "رفض غير سوبر/محمي/PUT قديم/no-op", "passed-unit-sqlite", "PermissionOverridesHandlerTests"),
    ("AC-powers-state", "مسودة/baseline/اعتماد صريح", "passed-unit-mock", "powersFormState.test.ts (6)"),
    ("AC-session-01.2", "اختبارات الجلسة", "passed-unit", "vitest session + powers = 21"),
]

# ربط حقيقي على PostgreSQL معزول qalaat_phase02_verify — 2026-11-04
ACCEPTANCE_LIVE = [
    ("LV-login-superadmin", "دخول سوبر أدمن + ManagePermissions", "passed-live", "_phase02_live_results.json"),
    ("LV-open-editor", "فتح محرر الصلاحيات عبر API/UI", "passed-live", "GET overrides + screenshots/01"),
    ("LV-deny-relogin", "Deny يمنع موروث بعد إعادة الدخول", "passed-live", "Order.Read removed"),
    ("LV-allow-relogin", "Allow يضيف صلاحية", "passed-live", "User.Create added"),
    ("LV-inherit-relogin", "Inherit يعيد افتراض الدور", "passed-live", "Order.Read restored"),
    ("LV-session-invalidate", "إبطال refresh بعد تغيير صلاحيات", "passed-live", "refresh 400"),
    ("LV-reject-non-super", "رفض إدارة صلاحيات من غير مخول", "passed-live", "chief → 403"),
    ("LV-reject-role-assign", "رفض تغيير الدور من غير سوبر", "passed-live", "Not_Authorized_To_Assign…"),
    ("LV-conflict-409", "تعارض جلستين → 409", "passed-live", "A=200 B=409"),
    ("LV-conflict-merge", "دمج بعد 409 يحفظ الطرفين", "passed-live", "deny+inherit retained"),
    ("LV-noop", "no-op لا يزيد الإصدار ولا يبطل جلسات", "passed-live", "v unchanged; refresh 200"),
    ("LV-create-powers", "إنشاء حساب ثم PATCH صلاحيات", "passed-live", "create+patch 200"),
    ("LV-protected", "حساب محمي canEdit=false", "passed-live", "admin id=1"),
    ("LV-ui-desktop", "سطح: خيارات ثلاثية + نتيجة فعلية + تعارض", "passed-live", "screenshots/01-02"),
    ("LV-ui-mobile", "هاتف: بلا تمرير أفقي + رسائل تعارض", "passed-live", "screenshots/03"),
]

STATUS = [
    ("منفّذ ومختبر (وحدة)", "احتساب؛ handlers SQLite؛ powersFormState؛ جلسة 01.2؛ PendingModelChangesGate"),
    ("مختبر ربط حي PostgreSQL معزول", "قاعدة qalaat_phase02_verify على localhost فقط؛ 24/24 سيناريو API؛ UI حقيقي ضد API:4200"),
    ("مختبر ببيانات محاكية للواجهة", "powersFormState — ليس إثبات ربط"),
    ("مخطط التجريبية", "migrations الحالية + SQL يدوي SCHEMA_REQUIRED؛ بلا توليد migration"),
    ("إغلاق تحقق", "أُزيل التجاهل العام لـPendingModelChanges؛ بوابة Development+علم false افتراضيًا+مضيف محلي+قاعدة مسموحة"),
    ("متبقٍ قبل الإنتاج", "تسوية فجوة ModelSnapshot وفق سياسة المشروع ثم تطبيق المخطط؛ تفويض مدير فرع"),
]

LIVE_ENV = [
    ("قاعدة", "qalaat_phase02_verify @ 127.0.0.1:5432 — ليست QalaatAlDamanDb/إنتاج"),
    ("API", "http://127.0.0.1:4200 — ConnectionStrings عبر env جلسة التشغيل فقط"),
    ("Panel", "http://127.0.0.1:5173 — VITE_API_BASE_URL محلي عبر env؛ لم يُعدَّل .env الإنتاج في المستودع"),
    ("مخطط", "_apply_phase02_schema.sql مطابق SCHEMA_REQUIRED + EF"),
    ("نتائج API", "_phase02_live_results.json — pass=24 fail=0"),
    ("لقطات", "screenshots/ — بيانات وهمية mandob_p02_*"),
]


def style_header(ws, headers):
    ws.append(headers)
    for col, _ in enumerate(headers, 1):
        c = ws.cell(1, col)
        c.fill = HEADER_FILL
        c.font = HEADER_FONT


def style_body(ws, ncols):
    for row in ws.iter_rows(min_row=1, max_row=ws.max_row, max_col=ncols):
        for cell in row:
            cell.border = THIN
            if cell.row > 1:
                cell.font = BODY_FONT
            cell.alignment = Alignment(wrap_text=True, vertical="top")
    for col in range(1, ncols + 1):
        vals = [str(c.value or "") for c in ws[get_column_letter(col)]]
        ws.column_dimensions[get_column_letter(col)].width = min(max(len(v) for v in vals) + 2, 70)
    ws.sheet_view.rightToLeft = True


def write_excel():
    wb = Workbook()
    sheets = [
        ("Endpoints", ["Method", "Path", "التخويل", "الحالة"], ENDPOINTS),
        ("AcceptanceUnit", ["id", "السيناريو", "الحالة", "الدليل"], ACCEPTANCE_UNIT),
        ("AcceptanceLive", ["id", "السيناريو", "الحالة", "الدليل"], ACCEPTANCE_LIVE),
        ("Status", ["التصنيف", "التفاصيل"], STATUS),
        ("LiveEnv", ["البند", "القيمة"], LIVE_ENV),
        (
            "Overrides",
            ["العنصر", "المعنى"],
            [
                ("UserPermissions", "سماح خاص"),
                ("UserPermissionDenials", "منع خاص"),
                ("Users.PermissionsVersion", "Concurrency token"),
            ],
        ),
        (
            "Compatibility",
            ["البند", "السلوك"],
            [
                ("PUT بدون expectedVersion", "يرفض (مطلوب)"),
                ("User.Update وحده لتغيير الدور", "يرفض — يحتاج سوبر أدمن"),
                ("PendingModelChangesWarning", "غير مفعّل افتراضيًا؛ تجربة محلية فقط ببوابة مشروطة"),
                ("SQL اليدوي", "لا يحدّث ModelSnapshot — تسوية قبل نشر الإنتاج"),
            ],
        ),
    ]
    first = True
    for title, headers, rows in sheets:
        ws = wb.active if first else wb.create_sheet(title)
        if first:
            ws.title = title
            first = False
        style_header(ws, headers)
        for r in rows:
            ws.append(list(r))
        style_body(ws, len(headers))
    out = ROOT / "Unified_Admin_Permissions.xlsx"
    wb.save(out)
    return out


def add_h(doc, text, level=1):
    p = doc.add_heading(text, level=level)
    p.alignment = WD_ALIGN_PARAGRAPH.RIGHT


def add_p(doc, text):
    p = doc.add_paragraph(text)
    p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    for run in p.runs:
        run.font.size = Pt(12)
        run.font.name = "Arial"


def write_word():
    doc = Document()
    t = doc.add_heading("مرحلة 02 — تحقق تشغيلي محلي معزول", 0)
    t.alignment = WD_ALIGN_PARAGRAPH.RIGHT

    add_h(doc, "بيئة الربط الحي")
    add_p(doc, "قاعدة PostgreSQL تجريبية qalaat_phase02_verify على 127.0.0.1 فقط. لم تُمس QalaatAlDamanDb ولا نُسخت بيانات إنتاج. المخطط الأساسي عبر EF migrations الحالية ثم SQL يدوي من SCHEMA_REQUIRED (_apply_phase02_schema.sql). بلا توليد migration.")
    add_p(doc, "API على http://127.0.0.1:4200 والواجهة على http://127.0.0.1:5173 بإعدادات جلسة معزولة (ConnectionStrings/VITE عبر env) دون تعديل إعدادات الإنتاج في المستودع.")

    add_h(doc, "نتائج السيناريوهات الحية (API)")
    add_p(doc, "24/24 ناجحة — انظر AcceptanceLive و_phase02_live_results.json: Deny/Allow/Inherit بعد إعادة الدخول؛ إبطال refresh؛ رفض غير مخول (403) ورفض تعيين دور؛ تعارض 409 ثم دمج الطرفين؛ no-op بدون زيادة إصدار ولا إبطال؛ إنشاء+PATCH صلاحيات؛ حساب محمي.")

    add_h(doc, "واجهة حقيقية + تصميم")
    add_p(doc, "دخول سوبر أدمن وفتح محرر الصلاحيات لمستخدم وهمي. الخيارات الثلاثة (حسب الدور/سماح/منع) والنتيجة الفعلية ظاهرة. شريط تعارض حي baseline v5 · server v6 مع أزرار مقارنة/دمج/تجاهل. سطح المكتب والهاتف بلا تمرير أفقي للصفحة. اللقطات في screenshots/ ببيانات وهمية.")

    add_h(doc, "اختبارات وحدة (منفصلة عن الربط الحي)")
    add_p(doc, "Backend Users: 13/13. Frontend session+powersFormState: 21/21.")

    add_h(doc, "إغلاق التحقق — PendingModelChanges وModelSnapshot")
    add_p(doc, "أُزيل التجاهل العام. التفعيل اختياري فقط: Development + Phase02LocalVerify:AllowPendingModelChanges=true (افتراضي false) + مضيف حلقي محلي + قاعدة مسموحة؛ اسم القاعدة وحده لا يكفي. الإعدادات العادية لا تتجاهل التحذير.")
    add_p(doc, "SQL اليدوي لا يحدّث EF ModelSnapshot. مسار ترقية الإنتاج يحتاج تسوية هذه الفجوة وفق سياسة المشروع (AGENTS.md) قبل النشر، ثم تطبيق المخطط والتحقق.")

    add_h(doc, "واجهة الموظف")
    add_p(doc, "أُخفيت أرقام baseline/server من الشاشة لصالح رسائل عربية؛ التفاصيل في التشخيص (console.debug / expose). شريط الإجراءات الثابت مع padding سفلي كافٍ على الهاتف.")

    add_h(doc, "متبقٍ")
    add_p(doc, "تسوية ModelSnapshot/migration وفق السياسة قبل أي نشر إنتاج. تفويض مدير فرع مؤجّل.")

    out = ROOT / "Unified_Admin_Permissions_Architecture.docx"
    doc.save(out)
    return out


def main():
    print(f"Wrote {write_excel()}")
    print(f"Wrote {write_word()}")


if __name__ == "__main__":
    main()
