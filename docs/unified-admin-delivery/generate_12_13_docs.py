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
ws.title = "12_Location"
style_header(ws, ["Item", "Rule", "Status"])
append_rows(ws, [
    ("Subject", "Motaba follower only", "Proven"),
    ("Storage", "SalesLocationPoints reuse", "Enhanced"),
    ("Ingest", "POST committee/locations/live + batch", "Shift required"),
    ("Admin GET", "/admin/FollowerTracking/latest", "FollowerLocation.Read"),
    ("Latest order", "max CapturedAt not ReceivedAt", "Implemented"),
    ("Status", "Live<=150s / Stale / NoShift / NoLocation", "Implemented"),
    ("UI", "Leaflet OSM + list + poll 15s", "Ready"),
    ("App loop", "Foreground 60s while on duty", "Wired"),
    ("Live verify", "API/Browser/Device", "Pending"),
])

ws2 = wb.create_sheet("13_Routes")
style_header(ws2, ["Item", "Rule", "Status"])
append_rows(ws2, [
    ("Admin GET", "/admin/FollowerTracking/{id}/route", "FollowerRoute.Read"),
    ("Day", "Baghdad calendar day", "Implemented"),
    ("Display", "Markers only — no fake polyline", "Implemented"),
    ("Gaps", "Announce >30min gaps", "UI note"),
    ("Pagination", "page/pageSize truncated flag", "Implemented"),
    ("History scope", "BranchId at ingest", "Documented"),
    ("Live verify", "pending", "Pending"),
])

ws3 = wb.create_sheet("Permissions")
style_header(ws3, ["Permission", "Opens"])
append_rows(ws3, [
    ("FollowerLocation.Read", "live map latest"),
    ("FollowerRoute.Read", "route history day"),
    ("OrderList.Read (app)", "shift + location ingest gate"),
])

ws4 = wb.create_sheet("CarryGaps_10_11")
style_header(ws4, ["Gap", "Status"])
append_rows(ws4, [
    ("Flutter delegate-notes write UI", "Open"),
    ("Mandob login lock when collection enabled", "Open (payment gate done)"),
    ("10/11 live verification", "Open"),
])

ws5 = wb.create_sheet("Tests")
style_header(ws5, ["Suite", "Result"])
append_rows(ws5, [
    ("FollowerLocationRulesTests", "unit"),
    ("vitest sections+nav", "unit"),
    ("Live screenshots", "pending"),
])

wb.save(ROOT / "Unified_Admin_Delivery_12_13.xlsx")

doc = Document()
doc.add_heading("12+13 — الموقع المباشر والمسارات", level=1)
doc.add_paragraph(
    "التتبع يعيد استخدام SalesLocationPoints/SalesShift. المتابع = Motaba. "
    "الإرسال من جلسة التطبيق مع دوام نشط؛ CapturedAt أساس آخر موقع. "
    "اللوحة: Leaflet+OSM، قائمة نصية، poll 15ث للموقع، مسار يومي markers بلا خط مضلل."
)
doc.add_heading("تدفق GPS", level=2)
doc.add_paragraph(
    "إذن موقع → بدء دوام → POST locations/live (accuracy/capturedAt/clientEventId) → "
    "تخزين BranchId من الدوام → GET latest للمخول → markers. "
    "المسار: GET route ليوم بغداد مرتبًا بالالتقاط."
)
doc.add_heading("نواقص 10/11", level=2)
doc.add_paragraph("تبقى مفتوحة: Flutter لكتابة ملاحظات المندوب؛ قفل دخول المندوب؛ التحقق الحي.")
doc.add_heading("14/15", level=2)
doc.add_paragraph("لم تبدأ.")
doc.save(ROOT / "Unified_Admin_Delivery_12_13_Architecture.docx")
print("wrote 12_13 docs")
