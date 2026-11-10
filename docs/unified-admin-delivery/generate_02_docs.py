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
    left=Side(style="thin"), right=Side(style="thin"),
    top=Side(style="thin"), bottom=Side(style="thin"),
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
style_header(ws, ["Method","Path","DTO","Response","Permissions","Scope","Errors","Consumer","Tests","Status"])
append_rows(ws, [
    ("GET","admin/DelegateFollowup/lists","branchId?","lists[]","DelegateFollowup.Read","allowed branches","403","Board","rules+live","Implemented/Tested"),
    ("GET","admin/DelegateFollowup/lists/{id}/summary","branchId","SummaryDto","Read","list.branch","403/404","Board","legal+due+committees","Implemented/Tested"),
    ("GET","admin/DelegateFollowup/lists/{id}/customers","branchId","CustomersDto","Read","list.branch","403/404","Board+CustomersPage","isLegal","Implemented/Tested"),
    ("GET","admin/DelegateFollowup/lists/{id}/customers/{cid}/payments","branchId","PaymentsDto","Read","order on list+branch","403/404","CustomersPage","unit+handler","Implemented/Tested"),
    ("GET/POST",".../board-operator-rating","score/note","Rating","Read/UpsertBoardRating","(OrderListId,RatingDate) UX","403/409 race→replace","Board","race handler","Implemented/Tested"),
    ("—","Create payment from this screen","—","—","—","مرجع الواجهة لا يستدعي Create","—","—","documented skip","N/A screens"),
])

ws2 = wb.create_sheet("FunctionalMatrix")
style_header(ws2, ["Item","Status","Evidence"])
append_rows(ws2, [
    ("Customer.IsLegal + migration","Implemented / Live-verified","20261108191345 on qalaat_local"),
    ("القسط المطلوب يستثني القانونية","Implemented / Tested","RequiredDue_ExcludesLegal + DataService"),
    ("عدّ قانونية + فلتر","Implemented","summary.legalAvailable + UI filter"),
    ("تقييمات اللجان الثلاث (قراءة)","Implemented / Tested","tables + Available=true empty ok"),
    ("عامل اللوحة UX يومي + race","Implemented / Tested","unique index + DbUpdateException retry"),
    ("صفحة تفاصيل الزبائن","Implemented","DelegateFollowupCustomersPage.vue"),
    ("تسديدات الزبون GET","Implemented","GetCustomerPayments"),
    ("إنشاء تسديد من الشاشة","N/A مطابقة شاشات","API قديم موجود؛ Vue لا يستدعيه"),
    ("DelegateState مكافئ","غير محسوم","OrderList بلا soft-active"),
    ("كتّاب تقييمات اللجان","ناقص خارج 02","تطبيقات المتابع/الفرع/المبيعات"),
    ("لقطات 360/768/1440","Blocked","تحتاج جلسة لوحة حية"),
])

ws3 = wb.create_sheet("Permissions")
style_header(ws3, ["Permission","Final","Notes"])
append_rows(ws3, [
    ("DelegateFollowup.Read","DelegateFollowup.Read","دخول + زبائن + تسديدات قراءة"),
    ("DelegateFollowup.UpsertBoardRating","UpsertBoardRating","لا يفتح التبويب وحده"),
    ("InstallmentPayment.Create","موجود عامًا","غير مربوط بشاشة المتابعة في 02"),
])

wb.save(ROOT / "Unified_Admin_Delivery_02_DelegateFollowup.xlsx")

doc = Document()
doc.add_heading("02 — استكمال متابعة المندوبين", level=1)
doc.add_paragraph("مطابقة مصدرية + تحقق معزول على qalaat_local. لا نشر. لا ملخص/تقييمات عامة/عمل متابعين.")
doc.add_heading("ملفات", level=2)
for f in [
    "Domain/Customer.IsLegal",
    "Domain/DelegateFollowupAggregate/{ListSupervision,FollowerDelegate,SalesEmployee}DayRating",
    "Application/DelegateFollowup/Common/DataService+Rules+Dtos",
    "Application/.../GetCustomerPayments",
    "Migration 20261108191345_DelegateFollowupLegalAndCommitteeRatings",
    "panel DelegateFollowupCustomersPage + returnContext + payments query",
    "Coverage_02_DelegateFollowup.md",
]:
    doc.add_paragraph(f, style="List Bullet")
doc.add_heading("متبقي", level=2)
doc.add_paragraph("كتّاب تقييمات اللجان؛ مكافئ DelegateState؛ لقطات تفاعلية حية؛ إنشاء تسديد من الشاشة غير مطلوب بمطابقة الواجهة.")
doc.save(ROOT / "Unified_Admin_Delivery_02_Architecture.docx")

cov = (ROOT / "Coverage_02_DelegateFollowup.md").read_text(encoding="utf-8")
if "## حالة الإغلاق" not in cov:
    (ROOT / "Coverage_02_DelegateFollowup.md").write_text(
        cov + """

## حالة الإغلاق (بعد التنفيذ)

| البند | الحالة |
|---|---|
| IsLegal + القسط المطلوب الصحيح | منفّذ ومختبر |
| تقييمات اللجان (جداول + قراءة) | منفّذ ومختبر (فارغ حتى يكتب المنتجون) |
| صفحة تفاصيل الزبائن + تسديدات GET | منفّذ |
| إنشاء تسديد من الشاشة | N/A — غير مستخدم في مرجع الواجهة |
| DelegateState | غير محسوم |
| لقطات حية | معلّق |
""",
        encoding="utf-8",
    )
print("OK")
