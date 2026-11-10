# -*- coding: utf-8 -*-
from pathlib import Path
from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from docx import Document

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


def style_header(ws, cols):
    for i, h in enumerate(cols, 1):
        c = ws.cell(1, i, h)
        c.fill, c.font, c.border = HEADER_FILL, HEADER_FONT, THIN
        c.alignment = Alignment(wrap_text=True, vertical="center")


def append_rows(ws, rows):
    for r in rows:
        ws.append(list(r))
        for cell in ws[ws.max_row]:
            cell.font, cell.border = BODY_FONT, THIN
            cell.alignment = Alignment(wrap_text=True, vertical="top")


wb = Workbook()
ws = wb.active
ws.title = "14_Overview"
style_header(
    ws,
    ["Metric", "Entity", "Rule", "Period", "Permission", "Link gate", "Status"],
)
append_rows(
    ws,
    [
        (
            "employeesOnShift",
            "SalesShift",
            "Mandob in scope + EndedAtUtc null",
            "now",
            "SalesOverview.Read",
            "SalesEmployee.Read",
            "Implemented",
        ),
        (
            "employeesOffShift",
            "SalesShift",
            "Mandob without active shift",
            "now",
            "SalesOverview.Read",
            "SalesEmployee.Read",
            "Implemented",
        ),
        (
            "liveLocations",
            "SalesLocationPoint",
            "Active shift + Live<=150s count only",
            "now",
            "SalesOverview.Read",
            "SalesEmployee.Read",
            "Implemented",
        ),
        (
            "salesToday",
            "Order",
            "Approved + SaleDate=business day Iraq",
            "cutoff 03:00 Baghdad",
            "SalesOverview.Read",
            "Order.Read",
            "Implemented",
        ),
        (
            "pendingSales",
            "SalesRequest",
            "!IsDeleted Status=Pending",
            "open set",
            "SalesOverview.Read",
            "SalesRequest.TabPending",
            "Implemented",
        ),
        (
            "newSalesRequests",
            "SalesRequest",
            "!IsDeleted Status=New",
            "open set",
            "SalesOverview.Read",
            "SalesRequest.TabIncoming",
            "Implemented",
        ),
        (
            "NOT salesRequest.Completed",
            "—",
            "Not a substitute for Order sales",
            "—",
            "—",
            "—",
            "Ruled out",
        ),
        ("Live verify", "—", "API/Browser/Device", "—", "—", "—", "Pending"),
    ],
)

ws2 = wb.create_sheet("15_Employees")
style_header(ws2, ["Field/Action", "Rule", "Status"])
append_rows(
    ws2,
    [
        ("Identity", "Role Mandob + completed + branch in scope (not Motaba)", "Implemented"),
        ("SellerId", "Order.SellerId matches employee for sales/ratings", "Documented"),
        ("shiftState", "Active|NoShift whitelist only", "Implemented"),
        ("LocationStatus", "FollowerLocationRules Live/Stale/NoLocation/NoShift", "Implemented"),
        ("Ratings", "CustomerRatingCalculator real+phantom + RatingTrendCalculator", "Implemented"),
        ("Buckets", "ممتاز/جيد/ضعيف/متلكئ/سيء", "Implemented"),
        ("CRUD/block", "Not on this screen; app-block via SalesFollowup", "Out of scope"),
        ("Route/map links", "Sales employee map deferred — no fake link", "Documented"),
        ("Live verify", "pending", "Pending"),
    ],
)

ws3 = wb.create_sheet("Contracts")
style_header(ws3, ["Method", "Path", "Permission", "Notes"])
append_rows(
    ws3,
    [
        ("GET", "/api/v1/admin/SalesOverview", "SalesOverview.Read", "branchId optional scoped"),
        ("GET", "/api/v1/admin/SalesEmployees", "SalesEmployee.Read", "search/shiftState/page/sort"),
        (
            "GET",
            "/api/v1/admin/SalesEmployees/{id}/rating-summary",
            "SalesEmployee.Read",
            "bucket optional; amounts gated",
        ),
    ],
)

ws4 = wb.create_sheet("CarryGaps")
style_header(ws4, ["Gap", "Batch", "Status"])
append_rows(
    ws4,
    [
        ("Flutter delegate-notes write UI", "10", "Open"),
        ("Mandob login lock when collection enabled", "11", "Open"),
        ("10–13 live verification", "10-13", "Open"),
        ("Device GPS day proof", "12/13", "Open"),
        ("14/15 live verification", "14/15", "Open"),
        ("16/17 sales followup expanded", "—", "Not started"),
    ],
)

ws5 = wb.create_sheet("Tests")
style_header(ws5, ["Suite", "Result"])
append_rows(
    ws5,
    [
        ("SalesWorkDayIraqTests", "unit"),
        ("SalesOverviewMetricKeysTests", "unit"),
        ("SalesEmployeeIdentityTests", "unit"),
        ("navigation.registry.test (saleOverview/employees ready)", "unit"),
        ("Live screenshots 360/768/1440", "pending"),
    ],
)

wb.save(ROOT / "Unified_Admin_Delivery_14_15.xlsx")

doc = Document()
doc.add_heading("14+15 — نظرة عامة للمبيعات وموظفو المبيعات", level=1)
doc.add_paragraph(
    "تحت قسم المبيعات الموحّد بصلاحيات قابلة للتخصيص — بلا لوحة SalesManager. "
    "لا إعادة بناء SalesRequests أو SalesFollowup. 16/17 لم تبدأ."
)
doc.add_heading("ملفات أساسية", level=2)
for line in [
    "Application/SalesOverview/* — مؤشرات مجمّعة DB",
    "Application/SalesEmployees/* — قائمة + ملخص تقييم",
    "Api/.../SalesOverviewController.cs + SalesEmployeesController.cs",
    "panel SalesOverview/ + SalesEmployees/ modules",
    "Permissions: SalesOverview.Read / SalesEmployee.Read (OutsideAll)",
]:
    doc.add_paragraph(line, style="List Bullet")

doc.add_heading("مصادر الحقيقة", level=2)
doc.add_paragraph(
    "الدوام: SalesShift.EndedAtUtc==null. الموقع: SalesLocationPoints + عتبات FollowerLocationRules. "
    "المبيعات المنجزة: Order Approved بـ SaleDate ليوم العمل (قطع 03:00 بغداد) — ليس SalesRequest.Completed. "
    "الطلبات: SalesRequest !IsDeleted بالحالة New/Pending. "
    "موظف المبيعات: دور Mandob ضمن الفروع؛ SellerId للبيع/التقييم."
)
doc.add_heading("التفويض والكاش", level=2)
doc.add_paragraph(
    "Overview.Read لا يفتح تفاصيل الطلبات/الموقع التاريخي/المبالغ. "
    "روابط المؤشرات تُفعَّل فقط عند صلاحية الوجهة. "
    "Query keys تشمل المستخدم والفرع والفلاتر؛ تبديل الحساب يغيّر المفتاح."
)
doc.add_heading("نواقص سابقة محفوظة", level=2)
doc.add_paragraph(
    "10: Flutter ملاحظات المندوب. 11: قفل دخول المندوب. 12/13: تحقق حي/جهاز. "
    "حالاتها تبقى مفتوحة — لم تُعلَن مكتملة."
)
doc.add_heading("16/17", level=2)
doc.add_paragraph("لم تبدأ.")
doc.save(ROOT / "Unified_Admin_Delivery_14_15_Architecture.docx")
print("wrote 14_15 docs")
