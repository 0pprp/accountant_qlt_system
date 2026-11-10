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
ws.title = "Identity"
style_header(ws, ["Rule", "Detail", "Status"])
append_rows(ws, [
    ("Eligibility", "SalesEmployeeAssignment active (branch+window)", "Decided"),
    ("Not enough alone", "Mandob / prior sale / GPS / login", "Ruled out"),
    ("Collection Mandob", "Role Mandob + OrderList.MandobId", "Separate"),
    ("Dual function", "Mandob + Assignment allowed independently", "Supported"),
    ("SellerId", "Historical sale attribution only", "Documented"),
    ("Backfill", "No auto-grant; ambiguous → review log", "Policy"),
])

ws2 = wb.create_sheet("16_Followup")
style_header(ws2, ["Item", "Status"])
append_rows(ws2, [
    ("List by assignment", "Fixed"),
    ("Detail KPIs", "Kept"),
    ("Manager rating -10..10", "Kept + assignment gate"),
    ("App block idempotent", "Fixed"),
    ("Block scope", "Sales app login/shift only"),
    ("Excel export customers", "Gap open"),
    ("Live verify", "Pending"),
])

ws3 = wb.create_sheet("17_Location")
style_header(ws3, ["Item", "Status"])
append_rows(ws3, [
    ("ActorKind SalesEmployee|Follower", "Migration"),
    ("GET /admin/SalesEmployeeTracking/latest", "SalesLocation.Read"),
    ("Ingest sales path assignment gate", "Implemented"),
    ("UI /sale/live-map poll 15s", "Ready"),
    ("Foreground-only tracking claim", "Documented"),
    ("No historical routes (18)", "Out of scope"),
    ("Live verify device", "Pending"),
])

ws4 = wb.create_sheet("CarryGaps")
style_header(ws4, ["Gap", "Batch", "Status"])
append_rows(ws4, [
    ("Flutter delegate-notes write", "10", "Open"),
    ("Mandob login lock collection", "11", "Open"),
    ("10–15 live verification", "10-15", "Open"),
    ("16/17 live verification", "16/17", "Open"),
    ("18/19", "—", "Not started"),
])

ws5 = wb.create_sheet("Tests")
style_header(ws5, ["Suite", "Result"])
append_rows(ws5, [
    ("SalesWorkforceTests", "unit"),
    ("SalesLocationActorIsolationTests", "unit"),
    ("SalesWorkDayIraq / Identity 14-15 regression", "unit"),
    ("navigation saleLiveMap ready", "unit"),
])

wb.save(ROOT / "Unified_Admin_Delivery_16_17.xlsx")

doc = Document()
doc.add_heading("16+17 — متابعة المبيعات + الموقع المباشر", level=1)
doc.add_paragraph(
    "هوية موظف المبيعات = SalesEmployeeAssignment. "
    "Mandob للجباية منفصل. SellerId لإسناد البيع التاريخي. "
    "لا إعادة بناء SalesFollowup؛ استكمال بوابات التكليف والحظر. "
    "الموقع: ActorKind على SalesLocationPoints + GET SalesEmployeeTracking/latest."
)
doc.add_heading("ملفات", level=2)
for line in [
    "Domain/SalesEmployeeAggregate/SalesEmployeeAssignment.cs",
    "SalesLocationActorKind + migration",
    "SalesWorkforce shared helper",
    "SalesEmployeeTracking/*",
    "panel SalesLiveMap/",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.add_heading("عزل", level=2)
doc.add_paragraph(
    "FollowerTracking يستبعد ActorKind=SalesEmployee. "
    "SalesLocation.Read لا يفتح مواقع المتابعين والعكس."
)
doc.add_heading("18/19", level=2)
doc.add_paragraph("لم تبدأ.")
doc.save(ROOT / "Unified_Admin_Delivery_16_17_Architecture.docx")
print("wrote 16_17 docs")
