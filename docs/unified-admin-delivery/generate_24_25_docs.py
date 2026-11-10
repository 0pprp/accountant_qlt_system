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
ws.title = "24_CommitteeTasks"
style_header(ws, ["Endpoint", "Request/Response", "Permission", "Actor/Effect", "Consumers", "Verify"])
append_rows(ws, [
    ("GET /admin/CommitteeTasks", "→ CommitteeTaskRowDto[]", "CommitteeTask.Read", "كل الصفوف مرتبة SortOrder,Id", "Panel oversight/committee-tasks", "Unit+API"),
    ("POST /admin/CommitteeTasks", "Body,IsEnabled → row", "CommitteeTask.Write", "SortOrder=max+1؛ ExpectedVersion=0", "Panel", "Unit"),
    ("PUT /admin/CommitteeTasks/{id}", "Body,IsEnabled,ExpectedVersion", "CommitteeTask.Write", "ConcurrencyStamp؛ رفض النسخة القديمة", "Panel", "Unit"),
    ("DELETE /admin/CommitteeTasks/{id}?expectedVersion", "—", "CommitteeTask.Write", "حذف مع concurrency", "Panel", "Unit"),
    ("GET /app/committee/tasks", "→ enabled Body only", "CommitteeTask.AppRead", "IsEnabled && Body!=''", "committee TasksPage", "Unit+code"),
])

ws2 = wb.create_sheet("25_AppNotes")
style_header(ws2, ["Endpoint", "Request/Response", "Permission", "Actor/Effect", "Consumers", "Verify"])
append_rows(ws2, [
    ("GET /admin/AppNotes", "page,unreadOnly,sourceApp,search → list+unreadCount", "AppNote.Read", "ترتيب CreatedAtUtc,Id desc", "Panel app-notes", "Unit"),
    ("GET /admin/AppNotes/unread-count", "→ int", "AppNote.Read", "عداد عام للصف Unread", "Panel", "Unit"),
    ("GET /admin/AppNotes/{id}", "→ detail", "AppNote.Read", "تفاصيل كاملة", "Panel", "Unit"),
    ("PUT /admin/AppNotes/{id}/read", "→ detail", "AppNote.Read", "Idempotent MarkRead", "Panel", "Unit"),
    ("PUT /admin/AppNotes/read-all", "→ changed count", "AppNote.Read", "تعليم كل Unread", "Panel", "Unit"),
    ("POST /app/committee/director-notes", "Body,Subject? → detail", "AppNote.Create", "SourceApp=Follower؛ الكاتب من الجلسة", "committee director note", "Unit+code"),
    ("POST /app/AppNotes", "Body,Subject? → detail", "AppNote.Create", "SourceApp من دور الجلسة (Follower/Delegate/BranchManager)", "delegate SendManagerNotePage", "Unit+code"),
])

ws3 = wb.create_sheet("States")
style_header(ws3, ["Module", "State", "Actions", "Notes"])
append_rows(ws3, [
    ("24 CommitteeTaskItem", "Enabled/Disabled (IsEnabled)", "Create/Update/Delete admin؛ App read enabled", "ليست دورة قبول/إنجاز"),
    ("25 AppNote", "Unread → Read", "Create field؛ MarkRead/MarkAll admin", "لا رد؛ حالة على الصف وليست لكل مستخدم"),
])

ws4 = wb.create_sheet("Delivery")
style_header(ws4, ["Layer", "24", "25"])
append_rows(ws4, [
    ("Implemented", "Yes — module separate from ManagementTasks", "Yes — separate from CustomerNotes/DelegateNotes"),
    ("Unit-tested", "Domain+handlers (create/concurrency/app filter)", "Domain+handlers (create/idempotent read/list/order)"),
    ("API-live", "Pending iso migrate this session", "Pending iso migrate this session"),
    ("Browser-live", "Page wired; screenshots Missing", "Page wired; screenshots Missing"),
    ("Device-verified", "TasksPage code→/app/committee/tasks; device Missing", "delegate note page wired; device Missing"),
    ("Missing", "Browser shots; API-live on iso", "Browser shots; API-live on iso; BranchManager UI app N/A"),
])

ws5 = wb.create_sheet("CarryGaps")
style_header(ws5, ["Gap", "Batch", "Status"])
append_rows(ws5, [
    ("22 Device-verified create buttons", "22", "Open"),
    ("23 Browser-live screenshots", "23", "Open"),
    ("23 standalone concurrency scenario", "23", "Open"),
    ("10–21 prior documented gaps", "10-21", "Open where not closed"),
    ("26+", "—", "Not started"),
])

wb.save(ROOT / "Unified_Admin_Delivery_24_25.xlsx")

doc = Document()
doc.add_heading("24+25 — مهام اللجنة + ملاحظات التطبيقات", level=1)
doc.add_paragraph(
    "24 نشرة مهام نصية (كتالوج تمكين) منفصلة عن ManagementTasks. "
    "25 صندوق وارد AppNotes Unread/Read منفصل عن CustomerNotes وFollowerDelegateNotes وشكاوى المفوض."
)
doc.add_heading("حدود الوحدات", level=2)
for line in [
    "Domain CommitteeTaskItem + AppNote (enums SourceApp/Status)",
    "Application/CommitteeTasks — Admin CRUD + App enabled list",
    "Application/AppNotes — Create/List/MarkRead/MarkAll/UnreadCount",
    "Api Admin CommitteeTasksController + AppNotesController",
    "Api App committee/tasks + director-notes → CreateAppNote(Follower)",
    "Api App AppNotes POST — SourceApp من الدور",
    "panel CommitteeTasksPage + AppNotesPage تحت لوحة الإدارة/oversight",
    "committee TasksPage → /app/committee/tasks؛ director note موجود",
    "delegate SendManagerNotePage → /app/AppNotes",
]:
    doc.add_paragraph(line, style="List Bullet")

doc.add_heading("انتقالات", level=2)
doc.add_paragraph(
    "24: إنشاء/تعديل Body+IsEnabled/حذف مع ExpectedVersion. التطبيق يقرأ المفعّل فقط. "
    "25: Create→Unread؛ MarkRead idempotent→Read؛ لا رد ولا إغلاق متعدد الحالات."
)
doc.add_heading("صلاحيات", level=2)
doc.add_paragraph(
    "OutsideAll: CommitteeTask.Read/Write/AppRead؛ AppNote.Read/Create. "
    "لا تُضاف إلى Permissions.All. قفل التبويب = Read فقط."
)
doc.add_heading("نواقص", level=2)
doc.add_paragraph(
    "API-live/Browser-live/Device-verified لهذه الجلسة Missing حيث لم تُشغَّل iso أو جهاز. "
    "نواقص 22–23 Browser/Device/concurrency تبقى مفتوحة. لا 26 ولا نشر إنتاج."
)
doc.save(ROOT / "Unified_Admin_Delivery_24_25_Architecture.docx")
print("Wrote Unified_Admin_Delivery_24_25.xlsx and Architecture.docx")
