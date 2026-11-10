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
ws.title = "Workflow"
style_header(ws, ["Question","Answer","Evidence"])
append_rows(ws, [
    ("Who authenticates?","Motaba/committee app — not director panel","Followers/PendingSales/Authenticate"),
    ("Panel actions","Read-only display of results + buckets","director-follower-work sales tab"),
    ("Proof of auth","FollowerSaleAuthentication row","new table + OrderId unique"),
    ("Order.Approve effect","Starts execution after auth write","Committee handler — separate from auth fact"),
    ("Not SalesRequest complete","Auth ≠ request completion","workflow md"),
])

ws2 = wb.create_sheet("Endpoints")
style_header(ws2, ["Method","Path","Auth","Status"])
append_rows(ws2, [
    ("GET","/api/v1/admin/FollowerWork/{id}/sales-review","FollowerSalesReview.Read","Implemented"),
    ("GET","/api/v1/admin/FollowerWork/{id}/remaining-customers","FollowerSalesReview.Read","Implemented"),
    ("POST","/api/v1/app/committee/pending-sales/{orderId}/authenticate","FollowerSalesReview.Authenticate","Implemented"),
])

ws3 = wb.create_sheet("Permissions")
style_header(ws3, ["Permission","Director/Monitor","Motaba","Opens"])
append_rows(ws3, [
    ("FollowerSalesReview.Read","seed yes","no","admin sales section"),
    ("FollowerSalesReview.Authenticate","no","seed yes","app write only"),
])

ws4 = wb.create_sheet("Gaps")
style_header(ws4, ["Item","Status"])
append_rows(ws4, [
    ("Delegate/employee shop ratings","null — no source tables"),
    ("Wrong-sale exception forward ≤6","deferred"),
    ("Live screenshots 360/768/1440","pending isolated DB"),
])

ws5 = wb.create_sheet("Files")
style_header(ws5, ["Layer","Path"])
append_rows(ws5, [
    ("Domain","FollowerSaleAuthentication"),
    ("App read","FollowerSalesReviewDataService"),
    ("App write","AuthenticateCommitteePendingSaleHandler"),
    ("Panel","FollowerSalesReviewPage.vue"),
    ("Migration","FollowerSaleAuthentications"),
])

wb.save(ROOT / "Unified_Admin_Delivery_06_FollowerSalesReview.xlsx")

doc = Document()
doc.add_heading("06 — مصادقة البيع", level=1)
doc.add_paragraph(
    "القسم يعرض نتائج مصادقة ميدانية كتبها المتابع. اللوحة لا تعتمد/ترفض. "
    "الكتابة عبر تطبيق اللجنة تسجّل FollowerSaleAuthentication ثم Approve للطلب. "
    "القراءة عبر sales-review وremaining-customers ضمن تكليف MotabaId الحالي."
)
doc.add_heading("التفويض", level=2)
doc.add_paragraph(
    "FollowerSalesReview.Read للوحة؛ Authenticate للتطبيق. "
    "قراءة القسم لا تمنح الكتابة ولا تفتح أقسام الملاحظات/الجباية/الموقع."
)
doc.add_heading("التزامن والكاش", level=2)
doc.add_paragraph(
    "Query keys تشمل session/branch/follower/bucket. "
    "تغيير المتابع أو الفرع يلغي السياق والبروفايل المفتوح. لا Hub جديد."
)
doc.add_heading("متبقي", level=2)
doc.add_paragraph("لقطات حية بعد migration؛ WrongSale forward؛ دفعة 07.")
doc.save(ROOT / "Unified_Admin_Delivery_06_Architecture.docx")
print("Wrote 06 excel+docx")
