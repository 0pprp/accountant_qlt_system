# -*- coding: utf-8 -*-
from pathlib import Path
from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from docx import Document

ROOT = Path(__file__).resolve().parent
HEADER_FILL = PatternFill("solid", fgColor="006A64")
HEADER_FONT = Font(color="FFFFFF", bold=True, name="Arial")
BODY_FONT = Font(name="Arial", size=11)
THIN = Border(left=Side(style="thin"), right=Side(style="thin"), top=Side(style="thin"), bottom=Side(style="thin"))

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
ws.title = "18_Routes"
style_header(ws, ["Field/Action", "Source", "Meaning", "New mapping", "Permission", "Acceptance"])
append_rows(ws, [
    ("Employee", "dropdown", "Sales employee", "userId + ActorKind=SalesEmployee", "SalesRoute.Read", "Location.Read alone denied"),
    ("Date", "single date", "Calendar day Baghdad", "IraqCalendar midnight→+1", "SalesRoute.Read", "Not sales 03:00 cutoff"),
    ("Points order", "CapturedAt", "Chronological", "RecordedAtUtc, Id", "—", "Late arrival does not reorder display"),
    ("Gaps", ">30min", "Announce only", "Gaps[] + UI note", "—", "No fake polyline"),
    ("Distance/speed", "not shown", "—", "Not invented", "—", "No road-distance claim"),
    ("Historical auth", "branch at capture", "No transfer to new branch", "point.BranchId ∈ reader scope", "SalesRoute.Read", "Ended assignment still readable if in scope"),
    ("Null BranchId points", "legacy", "Untrusted", "Excluded", "—", "No guess attribution"),
    ("Pagination", "page", "Stable pages", "Truncated explicit", "—", "No silent truncate"),
    ("API", "GET route", "Historical path", "/admin/SalesEmployeeTracking/{id}/route", "SalesRoute.Read", "Unit + contract"),
    ("UI", "/sale/routes", "Sales → routes", "SalesRoutes module", "SalesRoute.Read", "URL keeps employee+date+branch"),
])

ws2 = wb.create_sheet("19_Records")
style_header(ws2, ["Field", "Source meaning", "New source", "Permission", "Acceptance"])
append_rows(ws2, [
    ("Completed sale", "Approved Order", "ApprovalStatus=Approved", "Order.Read", "Completed request alone ≠ row"),
    ("Sale date", "SaleDate", "Order.SaleDate", "Order.Read", "Not CreatedAt-only"),
    ("Sell amount", "SellAmount", "Order.SellAmount", "Order.Read", "Currency د.ع"),
    ("Prepayment", "PrepaymentAmount", "Order.PrepaymentAmount", "Order.Read", "Not customer-wide"),
    ("Daily installment", "DailyInstallmentAmount", "Order.DailyInstallmentAmount", "Order.Read", "Plan amount"),
    ("Receipts", "sum payments of sale", "InstallmentPayments of Order", "Order.Read", "Not all customer payments"),
    ("Remaining", "sell−receipts", "computed per Order", "Order.Read", "Not multi-sale debt sum"),
    ("Seller", "SellerId", "historical SellerId", "Order.Read", "Visible after assignment end"),
    ("Customer", "Customer", "CustomerId/Name/Phone", "Order.Read / profile gates", "Profile link gated"),
    ("Linked request", "optional", "SalesRequest.LinkedOrderId", "Order.Read", "Link only — no duplicate Sale"),
    ("Export", "absent in source", "Not implemented", "—", "Does not close Followup Excel gap"),
    ("API", "list", "GET /admin/SalesRecords", "Order.Read", "Totals = filtered set"),
    ("UI", "/sale/completed", "SaleCompletedRoute", "Order.Read", "Filters in URL"),
])

ws3 = wb.create_sheet("Permissions")
style_header(ws3, ["Permission", "Grants", "Does NOT grant"])
append_rows(ws3, [
    ("SalesRoute.Read", "Historical sales employee routes", "Live map / follower routes / employee list"),
    ("SalesLocation.Read", "Live sales map", "Historical routes"),
    ("SalesEmployee.Read", "Employee directory", "Routes or live map"),
    ("Order.Read", "Completed sales list + sale view", "Write price/cancel/delete"),
])

ws4 = wb.create_sheet("Tests")
style_header(ws4, ["Suite", "Layer", "Result"])
append_rows(ws4, [
    ("SalesRouteDayBoundsTests", "Unit", "Pass expected"),
    ("SalesLocationActorIsolationTests", "Unit", "Pass"),
    ("SalesRecordsDefinitionTests", "Unit", "Pass expected"),
    ("navigation saleRoutes+saleOrders", "Unit FE", "ready"),
    ("API-live / Browser-live / Device", "Live", "Open — not claimed"),
])

ws5 = wb.create_sheet("CarryGaps")
style_header(ws5, ["Gap", "Batch", "Status"])
append_rows(ws5, [
    ("Flutter delegate-notes write", "10", "Open"),
    ("Mandob login lock collection", "11", "Open"),
    ("10–17 live verification", "10-17", "Open"),
    ("SalesFollowup Excel export customers", "16", "Open — not closed by 19"),
    ("18/19 live verification", "18/19", "Open"),
    ("20/21", "—", "Not started"),
])

wb.save(ROOT / "Unified_Admin_Delivery_18_19.xlsx")

doc = Document()
doc.add_heading("18+19 — مسارات موظفي المبيعات + المبيعات المنجزة", level=1)
doc.add_paragraph(
    "18 يعيد استخدام نقاط SalesLocationPoints (دفعة 17) بـ ActorKind=SalesEmployee "
    "وصلاحية SalesRoute.Read المستقلة. حدود اليوم تقويمية ببغداد وليست قطع بيع 03:00. "
    "19 يعرض Orders المعتمدة عبر إسقاط قراءة SalesRecords دون نطاق Sale موازٍ."
)
doc.add_heading("ملفات ومسؤوليات", level=2)
for line in [
    "SalesEmployeeTracking GetRoute + DataService.GetRouteAsync — قراءة المسار",
    "SalesRecords list query/handler/controller — قائمة المبيعات المنجزة",
    "panel SalesRoutes — /sale/routes + FollowerMapCanvas للعرض فقط",
    "panel SalesRecords — /sale/completed + تفاصيل عبر SaleView",
    "روابط موظفي المبيعات: live-map و routes عند الصلاحية",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.add_heading("تدفق القراءة والكاش", level=2)
doc.add_paragraph(
    "مفاتيح الاستعلام تتضمن الجلسة والفرع والموظف/البائع والفترة والصفحة. "
    "لا تحميل تاريخ الموقع مع قائمة المبيعات. لا fetch لكل نقطة عند فتح الخريطة — "
    "دفعة صفحة واحدة. تنظيف listeners عبر دورة حياة Vue للخريطة المشتركة."
)
doc.add_heading("مستندات وعودة", level=2)
doc.add_paragraph(
    "تفاصيل البيع عبر Order.Read. بروفايل الزبون عبر صلاحية البروفايل. "
    "return allowlist يضم SaleCompletedRoute وSaleRoutesRoute."
)
doc.add_heading("20/21", level=2)
doc.add_paragraph("لم تبدأ.")
doc.save(ROOT / "Unified_Admin_Delivery_18_19_Architecture.docx")
print("wrote 18_19 docs")
