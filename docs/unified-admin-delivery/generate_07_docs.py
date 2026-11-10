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
ws.title = "Rules"
style_header(ws, ["Rule", "Definition", "Evidence", "New"])
append_rows(ws, [
    ("Unit", "Customer within Motaba lists", "GetDecliningAsync PARTITION BY CustomerId", "FollowerDecliningRowDto.CustomerId"),
    ("Declining", "−10 / two bands within 7 days", "DirectorFollowerOpsDeclineRules", "FollowerDeclineRules"),
    ("Transitions", "ممتاز→ضعيف، جيد→متلكئ، ضعيف→سيء", "IsTargetTransition", "same"),
    ("Not weak cohort", "Not SalesRatings weak card", "UI hint + rules", "documented"),
    ("Snapshots", "Stored daily captures; today live upsert", "FollowerCustomerRatingSnapshots", "entity + migration"),
    ("No phantom history", "Do not invent prior days", "prompt + service", "CaptureToday only"),
    ("Assignment", "Current MotabaId lists", "OrderList.MotabaId", "ResolveFollowerScope"),
    ("Panel write", "None", "director tab read-only", "no mutate endpoint"),
])

ws2 = wb.create_sheet("Endpoints")
style_header(ws2, ["Method", "Path", "Auth", "Status"])
append_rows(ws2, [
    ("GET", "/api/v1/admin/FollowerWork/{id}/declining", "FollowerDeclining.Read", "Implemented"),
    ("GET", "/api/v1/app/committee/declining-customers", "Motaba JWT", "Aligned to decline rules"),
])

ws3 = wb.create_sheet("Fields")
style_header(ws3, ["Field", "Meaning", "Availability"])
append_rows(ws3, [
    ("from/to labels+scores", "Prior/current band", "required on row"),
    ("daysBetween", "CaptureDate delta", "0..7 for listed"),
    ("hasNote", "Follower note flag", "null until batch 08"),
    ("hasSaleAuth", "Auth on list customer", "from FollowerSaleAuthentications"),
])

ws4 = wb.create_sheet("Tests")
style_header(ws4, ["Case", "Status"])
append_rows(ws4, [
    ("Target transitions true/false", "unit"),
    ("Day 7 in / day 8 out", "unit"),
    ("Permission Declining ≠ Sales", "unit"),
    ("UI sections ready count=2", "vitest"),
    ("Live screenshots", "pending isolated DB"),
])

ws5 = wb.create_sheet("Files")
style_header(ws5, ["Layer", "Path"])
append_rows(ws5, [
    ("Rules", "Application/FollowerWork/Common/FollowerDeclineRules.cs"),
    ("Data", "FollowerDecliningDataService.cs"),
    ("Domain", "FollowerCustomerRatingSnapshot"),
    ("Admin API", "FollowerWorkController.GetDeclining"),
    ("App API", "GetCommitteeDecliningCustomersHandler"),
    ("Panel", "FollowerDecliningPage.vue"),
    ("Migration", "20261108202338_FollowerCustomerRatingSnapshots"),
])

wb.save(ROOT / "Unified_Admin_Delivery_07_FollowerDeclining.xlsx")

doc = Document()
doc.add_heading("07 — المتدنية (عمل المتابعين)", level=1)
doc.add_paragraph(
    "القسم يعرض زبائن هبط تقييمهم درجتين (−10) خلال 7 أيام على انتقالات محددة فقط. "
    "المقارنة من لقطات FollowerCustomerRatingSnapshots؛ لقطة اليوم تُحدَّث عند القراءة/تطبيق المتابع دون اختراع أيام سابقة. "
    "اللوحة قراءة فقط بصلاحية FollowerDeclining.Read. لا خلط مع مصادقة البيع أو ملاحظات أو فئة ضعيف العامة."
)
doc.add_heading("التفويض والنطاق", level=2)
doc.add_paragraph(
    "الفرع المسموح + MotabaId الحالي للقوائم. نقل التكليف يتبع القائمة الحالية. "
    "قراءة القسم لا تمنح كتابة ولا أقسامًا أخرى."
)
doc.add_heading("التدفق", level=2)
doc.add_paragraph(
    "CaptureToday → مقارنة مع prior ضمن lookback → IsDeclining → Response مع pagination. "
    "HasNote غير متوفر حتى دفعة 08. HasSaleAuth معلوماتي من سجلات المصادقة."
)
doc.add_heading("ملفات", level=2)
for line in [
    "FollowerDeclineRules / FollowerDecliningDataService",
    "GetFollowerDecliningQuery/Handler",
    "FollowerDecliningPage + useFollowerDecliningQuery",
    "Migration FollowerCustomerRatingSnapshots",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.save(ROOT / "Unified_Admin_Delivery_07_Architecture.docx")
print("OK")
