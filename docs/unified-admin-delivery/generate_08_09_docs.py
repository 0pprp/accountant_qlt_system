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
ws.title = "08_Notes"
style_header(ws, ["Item", "Rule", "Status"])
append_rows(ws, [
    ("Entity", "CustomerNotes / CustomerNote", "Implemented"),
    ("Admin GET", "/admin/FollowerWork/{id}/notes", "FollowerNotes.Read"),
    ("App POST", "/app/committee/customers/{id}/notes", "FollowerNotes.Write"),
    ("Panel write", "None", "Read-only"),
    ("HasNote", "Any CustomerNotes on customerId", "true/false wired"),
    ("Not delegate notes", "Separate batch 10", "Isolated"),
])

ws2 = wb.create_sheet("09_OldCustomers")
style_header(ws2, ["Item", "Rule", "Status"])
append_rows(ws2, [
    ("Entity", "LegalFollowupCases", "Implemented"),
    ("Definition", "Cases for FollowerUserId ≠ IsLegal alone", "Documented"),
    ("Admin GET", "/admin/FollowerWork/{id}/old-customers", "FollowerOldCustomers.Read"),
    ("Cancel", "POST /admin/LegalFollowups/{id}/cancel", "LegalFollowup.Cancel"),
    ("Cancel effect", "Cancelled + request→New", "No delete debt/customer"),
    ("Create cycle", "Sales legal tab", "Incomplete dependency"),
])

ws3 = wb.create_sheet("Permissions")
style_header(ws3, ["Permission", "Opens"])
append_rows(ws3, [
    ("FollowerNotes.Read", "notes list"),
    ("FollowerNotes.Write", "Motaba add note"),
    ("FollowerOldCustomers.Read", "old customers list"),
    ("LegalFollowup.Cancel", "cancel only"),
])

ws4 = wb.create_sheet("Tests")
style_header(ws4, ["Suite", "Result"])
append_rows(ws4, [
    ("FollowerNotesAndLegalRulesTests", "unit"),
    ("FollowerWork filter suite", "unit"),
    ("vitest sections+nav", "unit"),
    ("Live screenshots", "pending"),
])

wb.save(ROOT / "Unified_Admin_Delivery_08_09.xlsx")

doc = Document()
doc.add_heading("08+09 — الملاحظات والزبائن القديمة", level=1)
doc.add_paragraph(
    "08: CustomerNotes يكتبها المتابع؛ اللوحة تعرض فقط. HasNote في المتدنية = وجود أي ملاحظة على الزبون. "
    "09: LegalFollowupCases للمتابع؛ الإلغاء يعيد الطلب لغير مسند بلا حذف. "
    "دورة إنشاء القضية من تبويب القانونية ما زالت اعتمادًا ناقصًا خارج هذه الدفعة."
)
doc.add_heading("ملفات", level=2)
for p in [
    "CustomerNote + LegalFollowupCase",
    "FollowerNotesDataService / FollowerOldCustomersDataService",
    "CancelLegalFollowupHandler",
    "FollowerNotesPage / FollowerOldCustomersPage",
]:
    doc.add_paragraph(p, style="List Bullet")
doc.save(ROOT / "Unified_Admin_Delivery_08_09_Architecture.docx")
print("OK")
