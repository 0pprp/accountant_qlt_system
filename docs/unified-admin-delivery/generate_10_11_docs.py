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
ws.title = "10_DelegateNotes"
style_header(ws, ["Item", "Rule", "Status"])
append_rows(ws, [
    ("Writer", "Motaba / Follower role — not Mandob", "Proven"),
    ("Entity", "FollowerDelegateNotes (OrderListId + MandobUserId)", "Implemented"),
    ("Admin GET", "/admin/FollowerWork/{id}/delegate-notes", "FollowerDelegateNotes.Read"),
    ("App POST", "/app/committee/order-lists/{listId}/delegate-notes", "FollowerDelegateNotes.Write"),
    ("Panel write", "None", "Read-only"),
    ("Vs CustomerNotes", "Separate table/source", "Isolated"),
    ("Edit/Delete", "Forbidden after save", "Documented"),
    ("Live verify", "API/Browser/Device", "Pending"),
])

ws2 = wb.create_sheet("11_Collection")
style_header(ws2, ["Item", "Rule", "Status"])
append_rows(ws2, [
    ("Owner of flag", "OrderList.FollowerCollectionEnabled", "Implemented"),
    ("Admin GET", "/admin/FollowerWork/{id}/collection", "FollowerCollection.Read"),
    ("Toggle PATCH", "/admin/OrderLists/{id}/follower-collection", "ManageEnabled"),
    ("Body auth", "No branchId/followerId trust", "Enforced"),
    ("Version", "expectedVersion → 409", "Implemented"),
    ("Idempotent same state", "No effect / no version bump", "Implemented"),
    ("Field gate", "Mandob create payment blocked when enabled", "Implemented"),
    ("Totals", "DB sum filtered ≠ page-only", "Implemented"),
    ("Currency", "IQD no *1448", "Documented"),
    ("Live verify", "Isolated DB + device", "Pending"),
])

ws3 = wb.create_sheet("Permissions")
style_header(ws3, ["Permission", "Opens"])
append_rows(ws3, [
    ("FollowerDelegateNotes.Read", "admin delegate notes list"),
    ("FollowerDelegateNotes.Write", "Motaba add delegate note"),
    ("FollowerCollection.Read", "lists + payments read"),
    ("FollowerCollection.ManageEnabled", "toggle enable only"),
])

ws4 = wb.create_sheet("Consumers")
style_header(ws4, ["Consumer", "Uses"])
append_rows(ws4, [
    ("Panel FollowerDelegateNotesPage", "GET delegate-notes"),
    ("Panel FollowerCollectionPage", "GET collection + PATCH toggle"),
    ("Committee app POST delegate-notes", "field write"),
    ("App CreateInstallmentPayment", "Mandob gate"),
    ("Committee GetLists CollectionEnabled", "OrderList flag"),
])

ws5 = wb.create_sheet("Tests")
style_header(ws5, ["Suite", "Result"])
append_rows(ws5, [
    ("FollowerDelegateNotesAndCollectionRulesTests", "unit"),
    ("vitest FollowerWork sections", "unit"),
    ("vitest navigation.registry", "unit"),
    ("Live screenshots 360/768/1440", "pending"),
])

wb.save(ROOT / "Unified_Admin_Delivery_10_11.xlsx")

doc = Document()
doc.add_heading("10+11 — ملاحظات المندوبين والجباية", level=1)
doc.add_paragraph(
    "10: ملاحظة المندوب يكتبها المتابع عن القائمة/المندوب في FollowerDelegateNotes؛ اللوحة قراءة فقط؛ "
    "الكتابة عبر POST committee. ليست CustomerNotes. "
    "11: تمكين الجباية على OrderList؛ ManageEnabled مستقل؛ عند التفعيل يُرفض تسجيل دفعة المندوب على السيرفر؛ "
    "المدفوعات من InstallmentPayments مع تصنيف المصدر عبر CreatedBy؛ المجاميع من DB."
)
doc.add_heading("ملفات ومسؤوليات", level=2)
for p in [
    "FollowerDelegateNote + OrderList collection fields",
    "FollowerDelegateNotesDataService / FollowerCollectionDataService",
    "GetFollowerDelegateNotes / GetFollowerCollection",
    "SetFollowerCollectionEnabled + CreateInstallmentPayment gate",
    "FollowerDelegateNotesPage / FollowerCollectionPage",
]:
    doc.add_paragraph(p, style="List Bullet")

doc.add_heading("تدفق الملاحظة", level=2)
doc.add_paragraph(
    "متابع مخوّل → POST order-lists/{listId}/delegate-notes → تحقق التكليف → حفظ CreatedBy من الجلسة → "
    "لوحة المدير بـ GET delegate-notes ضمن فرع/تكليف."
)
doc.add_heading("تدفق تمكين الجباية حتى الدفعة", level=2)
doc.add_paragraph(
    "ManageEnabled → PATCH OrderLists/{id}/follower-collection (expectedVersion) → تحديث العلم + تدقيق → "
    "تطبيق المندوب يحاول CreateInstallmentPayment → السيرفر يرفض إن Mandob والقائمة مفعّلة؛ "
    "القراءة التاريخية للمدفوعات تبقى متاحة من اللوحة."
)
doc.add_heading("حالة القبول", level=2)
doc.add_paragraph("10: منفَّذ / مختبر وحدويًا / مثبت حيًا: ناقص / ناقص: Device-verified للكتابة الميدانية.")
doc.add_paragraph("11: منفَّذ / مختبر وحدويًا / مثبت حيًا: ناقص / ناقص: تطبيق migrations على قاعدة معزولة + تحقق جهاز.")
doc.add_paragraph("12/13: لم تبدأ (live-map / routes = not_implemented).")

doc.save(ROOT / "Unified_Admin_Delivery_10_11_Architecture.docx")
print("wrote xlsx+docx")
