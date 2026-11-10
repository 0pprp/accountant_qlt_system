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
ws.title = "Equations"
style_header(ws, ["Metric","Numerator/Rule","Denom","Period","Include","Exclude","SourceFile","Permission","Test","Status"])
append_rows(ws, [
    ("Customer score","ClassifySettledDays / ClassifyActiveRate","sales*days","asOf Baghdad","open+data","null sale","CustomerRatingCalculator","Read","unit thresholds","Implemented"),
    ("Active headline avg","mean scores active cohort","count rated","asOf","payment≤30d + remaining>0","settled/inactive","ActiveRatingCohort","Read","unit cohort","Implemented"),
    ("Period avg","mean scores sale-in-window","count","sale [from,to] eval→today","dated sales","undated","RatingTrendCalculator","Read","unit empty","Implemented"),
    ("List avg label","≥7/≥3/≥0/>−7.5","—","—","—","—","RatingAggregator=DF ClassifyAverageLabel","Read","shared assert","Implemented"),
    ("Month new","count open + sale in month","—","calendar month Baghdad","remaining>0","—","MonthBucket","Read","documented","Implemented"),
    ("Month zeroed","count remaining0 + lastPay in month","—","calendar month","—","—","MonthBucket","Read","documented","Implemented"),
    ("Direction","end−start vs ±0.05","—","chart series","—","—","RatingAggregator.DirectionFromSeries","Read","unit","Implemented"),
])

ws2 = wb.create_sheet("Endpoints")
style_header(ws2, ["Method","Path","Auth","Notes","Status"])
append_rows(ws2, [
    ("GET","/api/v1/admin/ManagementRatings/branches","Read","hub cards + month","Implemented"),
    ("GET","/api/v1/admin/ManagementRatings/catalog","Read","lists40d + followers + employees","Implemented"),
    ("GET","/api/v1/admin/ManagementRatings/period-summary","Read","scope/resource validated","Implemented"),
    ("GET","/api/v1/admin/ManagementRatings/trend","Read","branch/list/follower/employee","Implemented"),
    ("GET","/api/v1/admin/ManagementRatings/customers","Read+DetailsRead","paginated; legal mask","Implemented"),
    ("GET","/api/v1/admin/ManagementRatings/list-payments-sum","Read","list scoped","Implemented"),
])

ws3 = wb.create_sheet("Permissions")
style_header(ws3, ["Permission","Director","Monitor","Opens tab","Notes"])
append_rows(ws3, [
    ("ManagementRatings.Read","yes","yes","yes","hub+trend"),
    ("ManagementRatings.DetailsRead","yes","yes","no","customers; needs Read too"),
    ("Export","n/a","n/a","n/a","not in source UI"),
])

ws4 = wb.create_sheet("Files")
style_header(ws4, ["Layer","Path"])
append_rows(ws4, [
    ("Shared calc","Application/SalesRatings/Common/*"),
    ("App","Application/ManagementRatings/**"),
    ("Api","ManagementRatingsController.cs"),
    ("Panel","modules/ManagementRatings/**"),
    ("Docs","04-ratings-field-matrix.md + Coverage_04_Ratings.md"),
])

wb.save(ROOT / "Unified_Admin_Delivery_04_Ratings.xlsx")

doc = Document()
doc.add_heading("04 — التقييمات العامة", level=1)
doc.add_paragraph(
    "لوحة تقييمات المدير/المراقب داخل الإدارة الموحدة. "
    "الحسابات المشتركة في SalesRatings؛ قراءة البيانات في ManagementRatingsDataService. "
    "الكاش عبر Vue Query keys (جلسة/فرع/نطاق) — لا module-level cache القديم."
)
doc.add_heading("تدفق", level=2)
doc.add_paragraph(
    "branches → اختيار فرع → catalog + trend → نقر فئة → customers (Read+DetailsRead). "
    "resourceId يُتحقق أنه يتبع الفرع."
)
doc.add_heading("عزل عن المتابعة", level=2)
doc.add_paragraph(
    "نشاط القائمة 40ي للكتالوج فقط؛ عنوان التقييم 30ي. "
    "لا تقييم عامل اللوحة ولا SalesFollowup. لا دفعة 05."
)
doc.add_heading("متبقي", level=2)
doc.add_paragraph("لقطات تفاعلية؛ فهارس بعد قياس؛ بروفايل زبون عميق اختياري خارج النطاق الأدنى.")
doc.save(ROOT / "Unified_Admin_Delivery_04_Architecture.docx")
print("Wrote 04 excel+docx")
