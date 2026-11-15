# -*- coding: utf-8 -*-
"""Generate Excel API contract + Word architecture for Management Sales Map (34)."""
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from docx import Document
from docx.shared import Pt

ROOT = Path(__file__).resolve().parent
OUT = ROOT / "evidence" / "34-sales-map"
OUT.mkdir(parents=True, exist_ok=True)

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
ws.title = "Endpoints"
style_header(
    ws,
    [
        "Method+Path",
        "Query/Body",
        "Response",
        "Permission",
        "Errors",
        "Consumers",
        "Verify",
    ],
)
append_rows(
    ws,
    [
        (
            "GET /api/v1/admin/ManagementSalesMap/provinces",
            "—",
            "{ items: ProvinceCard[] }",
            "ManagementSalesMap.Read",
            "403 Forbidden",
            "Panel ManagementSalesMapPage",
            "Unit+API",
        ),
        (
            "GET /api/v1/admin/ManagementSalesMap/provinces/{provinceId}/lists",
            "provinceId route",
            "{ items: ListOption[] } letter أ…",
            "ManagementSalesMap.Read",
            "403; empty if خارج النطاق",
            "Panel list chips",
            "Unit+API",
        ),
        (
            "GET /api/v1/admin/ManagementSalesMap/provinces/{provinceId}/points",
            "listIds[]? status=0|1|2|3",
            "{ items: Point[] }",
            "ManagementSalesMap.Read",
            "403; بدون موقع لا يُعاد",
            "SalesMapCanvas",
            "Unit+API",
        ),
        (
            "GET /api/v1/admin/ManagementSalesMap/orders/{orderId}",
            "orderId route",
            "SalesMapOrderDetailDto",
            "ManagementSalesMap.Read",
            "403; 404 خارج النطاق/غير مؤهل",
            "SalesMapDetailCard",
            "Unit+API",
        ),
    ],
)

ws2 = wb.create_sheet("StatusRules")
style_header(ws2, ["Code", "Label", "Rule", "Filter tab"])
append_rows(
    ws2,
    [
        ("3", "قانونية", "Customer.IsLegal أولوية مطلقة", "Legal / All"),
        ("1", "مستمر", "ActiveCustomerRules 30ي على آخر تسديد للمبيع؛ ليس قانوني", "Continuous / All"),
        ("2", "متوقف", "غير قانوني وغير نشط", "Stopped / All"),
        ("null", "غير محدد", "بلا SaleDate وبلا آخر تسديد وليس قانوني", "All فقط"),
    ],
)

ws3 = wb.create_sheet("FiltersFields")
style_header(ws3, ["Surface", "Field", "Source", "Notes"])
append_rows(
    ws3,
    [
        ("Point", "orderId", "Order.Id Approved", "وحدة النقطة"),
        ("Point", "lat/lng", "Order.Location", "صالح ≠ null/(0,0)/خارج المدى"),
        ("Point", "listLetter", "OrderList.Id asc → أ،ب…", "ضمن المحافظة"),
        ("Point", "statusCode/glyph", "StatusRules", "١/٢/٣/؟"),
        ("Point", "ratingTier/color", "CustomerRatingCalculator على أرقام المبيع", "ليس تقييم موظف"),
        ("Detail", "sell/paid/remaining", "SellAmount + مجموع دفعات Order", "—"),
        ("Detail", "lastPayment*", "أحدث دفعة لهذا OrderId", "أو لا يوجد تسديد"),
        ("Province", "with/without location", "CountSalesAsync", "وهمي يُحسب بدون موقع ولا يُرسم"),
        ("Lists", "eligibility", "DelegateFollowupDataService.ListActiveListsAsync", "نفس 40ي+متبقٍ"),
    ],
)

ws4 = wb.create_sheet("Permissions")
style_header(ws4, ["Key", "Default matrix", "UI gate", "Notes"])
append_rows(
    ws4,
    [
        (
            "ManagementSalesMap.Read",
            "Director+Monitor OutsideAll ready",
            "sidebar + route + page locked",
            "صلاحية واحدة؛ Allow/Deny في محرر الحساب",
        ),
    ],
)

ws5 = wb.create_sheet("Delivery")
style_header(ws5, ["Layer", "Status", "Evidence"])
append_rows(
    ws5,
    [
        ("SOURCE_MATRIX", "Locked", "evidence/34-sales-map/SOURCE_MATRIX.md"),
        ("API CQRS", "Implemented", "Application/ManagementSalesMap + Controller"),
        ("Unit rules", "Implemented", "SalesMapRulesTests"),
        ("Panel Vue", "Implemented", "modules/ManagementSalesMap + nav"),
        ("API-live iso", "Pending this session", "evidence/34-sales-map/api-live.json"),
        ("Browser-live", "Pending this session", "screenshots under evidence/34-sales-map"),
    ],
)

xlsx_path = OUT / "ManagementSalesMap_API.xlsx"
wb.save(xlsx_path)

doc = Document()
style = doc.styles["Normal"]
style.font.name = "Arial"
style.font.size = Pt(11)

doc.add_heading("خريطة المبيعات — معمارية 34", level=0)
doc.add_paragraph(
    "تبويب تحت لوحة الإدارة: كروت محافظات → خريطة نقاط Order بموقع صالح، "
    "فلاتر قوائم/حالة، حرف قائمة + رمز حالة، لون تقييم الزبون، تجميع Leaflet markercluster/spiderfy، "
    "وتفاصيل مالية عند الضغط. صلاحية واحدة: ManagementSalesMap.Read."
)

doc.add_heading("خريطة الملفات", level=1)
files = [
    ("api-main/.../ManagementSalesMapController.cs", "HTTP surface admin"),
    ("Application/ManagementSalesMap/Common/*", "Dtos, Geo, Letters, StatusRules, DataService, Errors"),
    ("Application/ManagementSalesMap/Queries/Admin/*", "provinces/lists/points/detail handlers"),
    ("tests/.../SalesMapRulesTests.cs", "قواعد حالة/حروف/إحداثيات"),
    ("panel/.../ManagementSalesMap/*", "صفحة + canvas + detail + queries"),
    ("navigation.registry + sections", "managementSalesMap / salesMap ready"),
    ("RoleDefaultMatrix", "Director+Monitor OutsideAll"),
]
table = doc.add_table(rows=1, cols=2)
table.rows[0].cells[0].text = "ملف"
table.rows[0].cells[1].text = "مسؤولية"
for path, role in files:
    row = table.add_row().cells
    row[0].text = path
    row[1].text = role

doc.add_heading("قواعد الحالة والتقييم", level=1)
doc.add_paragraph(
    "قانونية (IsLegal) تسبق كل شيء → 3. وإلا ActiveCustomerRules (30 يوم على آخر تسديد للمبيع) → 1. "
    "وإلا متوقف → 2. بلا تواريخ وليس قانوني → غير محدد ويظهر في «الكل» فقط."
)
doc.add_paragraph(
    "لون الدبوس من CustomerRatingCalculator على أرقام هذا المبيع (excellent/good/weak/lagging/bad)، "
    "وليس تقييم موظف المبيعات."
)

doc.add_heading("التجميع والواجهة", level=1)
doc.add_paragraph(
    "Leaflet + leaflet.markercluster: spiderfyOnMaxZoom، disableClusteringAtZoom=18، "
    "أيقونة divIcon بالحرف+الرقم. الموبايل: ورقة تفاصيل سفلية؛ سطح المكتب: بطاقة جانبية."
)

doc.add_heading("نطاق الفروع", level=1)
doc.add_paragraph(
    "المحافظات والقوائم من فروع ضمن نطاق الحساب (OutsideAll للمدير/المراقب حسب القالب). "
    "قوائم المحافظة تطابق أهلية DelegateFollowup لنفس الفروع."
)

docx_path = OUT / "ManagementSalesMap_Architecture.docx"
doc.save(docx_path)
print("Wrote", xlsx_path)
print("Wrote", docx_path)
