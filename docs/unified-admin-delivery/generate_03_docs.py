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
ws.title = "Metrics"
style_header(ws, ["Display","Key","Meaning","DateBasis","Included","Excluded","NewSource","Permission","Test"])
append_rows(ws, [
    ("الواردات","receipts","Σ دفعات غير قانونيين","InstallmentPayment.Date","Approved orders","IsLegal","InstallmentPayments","ManagementSummary.Read","unit+manual"),
    ("الصرفيات","expenses","Σ صرفيات الفرع","CreatedAt Baghdad","Expense rows","—","Expense.TotalAmount","Read","unit rules"),
    ("سعر البيع","salePrice","Σ SellAmount","SaleDate/CreatedAt","Approved","—","Order.SellAmount","Read","unit"),
    ("سعر الشراء","purchasePrice","Σ BuyAmount للمبيعات","SaleDate/CreatedAt","Approved sales","≠ buys","Order.BuyAmount","Read","≠ buys assert"),
    ("المشتريات","buys","Σ فواتير شراء","CreatedAt Baghdad","Purchase","—","Purchase.TotalAmount","Read","unit"),
    ("المبلغ الكلي","boxTotalAmountDenar","Σ Safe remaining","as-of","allowed branches","not Abu-Sajjad lists","Safe sum adapted","Safe.Read or Forbidden","documented adapt"),
    ("المبلغ الفعلي","treasuryAmountDenar","خزين/قاصة أو أعلى رصيد","as-of","per branch pick","—","Safe adapted","Safe.Read or Forbidden","documented adapt"),
])

ws2 = wb.create_sheet("Endpoints")
style_header(ws2, ["Method","Path","Query","Response","Permission","Errors","Status"])
append_rows(ws2, [
    ("GET","/api/v1/admin/ManagementSummary/matrix","year,month,metric,branchId?","ManagementSummaryMatrixResponse","ManagementSummary.Read","InvalidMetric/Period/Branch","Implemented"),
    ("GET","/api/v1/admin/ManagementSummary/details","year,month,day,metric,branchId,page,pageSize","ManagementSummaryDetailsResponse","ManagementSummary.Drilldown","InvalidDay/BranchRequired","Implemented"),
])

ws3 = wb.create_sheet("Permissions")
style_header(ws3, ["Permission","RoleDefault Director","Opens tab","Notes"])
append_rows(ws3, [
    ("ManagementSummary.Read","yes","yes","only Read opens summary"),
    ("ManagementSummary.Drilldown","yes","no","details only; no implicit Read"),
    ("Safe.Read","yes (Director template)","—","box cards; Forbidden without it"),
    ("sale summary perms","no","no","does not unlock admin summary"),
])

ws4 = wb.create_sheet("SourceFiles")
style_header(ws4, ["Layer","Path","Role"])
append_rows(ws4, [
    ("Application","ManagementSummary/Common/*","rules, calendar, data service, DTOs"),
    ("Application","ManagementSummary/Queries/Admin/GetMatrix|GetDetails","CQRS"),
    ("Api","Controllers/V1/AdminArea/ManagementSummaryController.cs","thin controller"),
    ("Panel","modules/ManagementSummary/*","filters, matrix, details, page"),
    ("Nav","Core/navigation + Management/sections+router","ready + lock + /summary alias"),
    ("Docs","03-summary-field-matrix.md + Coverage_03_Summary.md","definitions"),
])

ws5 = wb.create_sheet("Tests")
style_header(ws5, ["Case","Status","Evidence"])
append_rows(ws5, [
    ("metric allowlist + purchasePrice≠buys","pass","ManagementSummaryRulesTests"),
    ("Baghdad month/day half-open bounds","pass","IraqCalendar tests"),
    ("nav Read vs Drilldown vs sales followup","pass","navigation.registry.test.ts"),
    ("interactive screenshots 360/768/1440","pending","evidence/03-summary/"),
    ("batch 04 ratings","not started","explicitly out of scope"),
])

wb.save(ROOT / "Unified_Admin_Delivery_03_Summary.xlsx")

doc = Document()
doc.add_heading("03 — الملخص الإداري", level=1)
doc.add_paragraph(
    "مصفوفة أيام×فروع للمقاييس receipts/expenses/salePrice/purchasePrice/buys "
    "ضمن لوحة الإدارة الموحدة. ليست Dashboard عامًا ولا ملخص المبيعات."
)
doc.add_heading("مسارات الملفات", level=2)
for f in [
    "Application/ManagementSummary/Common/{IraqCalendar,Rules,Access,Errors,Dtos,DataService,Metric}",
    "Application/ManagementSummary/Queries/Admin/GetMatrix|GetDetails",
    "Api/.../ManagementSummaryController.cs",
    "panel-admin-main/.../modules/ManagementSummary/**",
    "Management router alias /management/summary → oversight/summary",
    "Permissions.ManagementSummary OutsideAll + Director RoleDefaultMatrix",
]:
    doc.add_paragraph(f, style="List Bullet")

doc.add_heading("تدفق القراءة", level=2)
doc.add_paragraph(
    "الواجهة تقرأ الفلاتر من URL المُتحقَّق → matrix بـ Read → نقر الخلية يستدعي details بـ Drilldown "
    "بنفس شروط التجميع (مقياس/يوم/فرع). لا prefetch لكل الخلايا. إلغاء الطلب عند تبديل الفلاتر عبر signal."
)
doc.add_heading("حساب الفترات", level=2)
doc.add_paragraph(
    "Asia/Baghdad؛ بداية شاملة ونهاية غير شاملة لحدود CreatedAt؛ "
    "InstallmentPayment.Date و Order.SaleDate كتاريخ محلي يومي. لا إعادة حساب أرصدة."
)
doc.add_heading("علاقة المحاسبة", level=2)
doc.add_paragraph(
    "يعتمد كيانات Order/InstallmentPayment/Expense/Purchase/Safe الحالية للقراءة فقط. "
    "لا جدول Summary منسوخ. بطاقات القاصة تكييف Safe وليست نسخًا حرفيًا لصناديق أبو سجاد/القوائم."
)
doc.add_heading("متبقي", level=2)
doc.add_paragraph("تكييف boxTotal الحرفي؛ لقطات تفاعلية؛ فهارس بعد قياس الحمل. لا دفعة 04.")
doc.save(ROOT / "Unified_Admin_Delivery_03_Architecture.docx")

print("Wrote Unified_Admin_Delivery_03_Summary.xlsx and Architecture.docx")
