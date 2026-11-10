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
ws.title = "22_Endpoints"
style_header(ws, ["Endpoint", "Fields", "Permission", "Status/Effect", "Consumers", "Verify"])
append_rows(ws, [
    ("POST /app/SalesRequests/submit/{source}", "SubmissionKey,BranchId,Name,Phone,Address,Product,Note?", "SubmitEmployee|Delegate|Follower", "New unassigned; no Order; CreatedBy=session; Source=route", "Employee/Delegate/Committee apps", "Unit+API-live"),
    ("GET /app/SalesRequests/submission-branches/{source}", "—", "same Submit*", "UserBranches only", "3 apps", "API"),
    ("GET /admin/SalesRequests + match-hits", "filters", "Tab* / MatchRead", "4 match cards; states", "Panel SalesRequests", "Browser prior"),
    ("POST /admin/SalesRequests/{id}/assign", "target,version", "Assign", "A loses / B gains", "Panel", "API-live"),
    ("POST …/ensure-draft", "—", "AppLinkDraft", "idempotent LinkedOrder", "Employee app", "Unit/API"),
    ("app/committee/legal-followups/*", "GPS, docs, complete", "FieldRead/Upload/Complete", "InProgress→Completed", "Committee app", "Unit rules"),
    ("GET /admin/LegalFollowups/{id}/documents/{docId}/file", "—", "LegalFollowup.Read + branch", "blob preview", "Panel SalesLegal", "API"),
])

ws2 = wb.create_sheet("23_Tasks")
style_header(ws2, ["Action", "From→To", "Permission", "Actor rule", "Status"])
append_rows(ws2, [
    ("Create/Assign", "→Sent", "Create", "creator", "Implemented"),
    ("Accept", "Sent/Transferred→Accepted", "Accept", "current holder", "Implemented"),
    ("Reject", "→Returned", "Reject", "holder + reason", "Implemented"),
    ("Transfer", "Accepted→Transferred", "Transfer", "delegate→branch", "Implemented"),
    ("Complete self", "Accepted→Completed", "Complete", "holder==assignee no active foreign transfer", "Implemented"),
    ("Complete after transfer", "Accepted→AwaitingDelegatedConfirm", "Complete", "branch holder", "Implemented"),
    ("Confirm", "AwaitingDelegatedConfirm→Completed", "Confirm", "original assignee only", "Implemented"),
    ("Resend", "Returned→Sent", "Resend", "original assignee", "Implemented"),
    ("InboxForMe", "filter", "Read or action", "holder OR assignee on Returned/AwaitingConfirm", "Unit"),
    ("Attachment download", "file", "ReadAttachment", "CanSeeTask + task link", "Implemented"),
])

ws3 = wb.create_sheet("Apps")
style_header(ws3, ["App/Screen", "Button", "API source", "Status"])
append_rows(ws3, [
    ("sales_employee_app Home", "طلب مبيع جديد", "EmployeeSubmitted", "Implemented"),
    ("delegate_app Home", "طلب مبيع جديد", "Delegate", "Implemented"),
    ("committee SalesRequestsPage", "نموذج الإرسال", "Follower", "Implemented"),
    ("committee LegalFollowups", "موقع/رفع/إكمال", "Field*", "Implemented"),
    ("Panel ManagementTasks", "إدارة", "Admin ManagementTasks", "Implemented"),
    ("Delegate مهامي", "inboxOnly", "InboxForMe", "Implemented"),
    ("Branch مهامي", "inboxOnly", "InboxForMe", "Implemented"),
])

ws4 = wb.create_sheet("Delivery")
style_header(ws4, ["Layer", "22", "23"])
append_rows(ws4, [
    ("Implemented", "Yes", "Yes"),
    ("Unit-tested", "Yes (29 focused + 3 Flutter forms)", "Yes (Domain + InboxRules)"),
    ("API-live", "qalaat_22_23_iso PASS", "self-complete + transfer→confirm PASS"),
    ("Browser-live", "Code ready; screenshots Missing this session", "Page exists; screenshots Missing"),
    ("Device-verified", "Missing", "N/A web primary"),
    ("Missing", "Device proof; browser create-button shots", "Browser shots; standalone concurrency scenario"),
])

ws5 = wb.create_sheet("CarryGaps")
style_header(ws5, ["Gap", "Batch", "Status"])
append_rows(ws5, [
    ("10–19 prior live gaps where documented", "10-19", "Open"),
    ("16 Followup Excel", "16", "Open"),
    ("21 field complete + docs preview", "21→22", "Closed in 22"),
    ("Device-verified create buttons", "22", "Missing"),
    ("24+", "—", "Not started"),
])

wb.save(ROOT / "Unified_Admin_Delivery_22_23.xlsx")

doc = Document()
doc.add_heading("22+23 — تكامل طلبات البيع + المهام الإدارية", level=1)
doc.add_paragraph(
    "دفعة استكمال وتكامل وليست إعادة بناء. 22 يربط إنشاء الطلب من التطبيقات الثلاثة بمسار Submit "
    "idempotent مع Source من المسار وCreatedBy من الجلسة، ويغلق القانونية الميدانية ومعاينة المستمسكات. "
    "23 يراجع ManagementTasks الموجود: فصل مهامي/إدارة، انتقالات Confirm-after-transfer، InboxForMe أوسع من holder، مرفقات محروسة."
)
doc.add_heading("ملفات ومسؤوليات", level=2)
for line in [
    "Application/SalesRequests/Commands/App/Submit/* — إنشاء + بصمة + قفل",
    "Api/.../AppArea/SalesRequestsController — submit/{source}",
    "sales_employee_app / delegate_app / committee — NewSalesRequestPage",
    "Application/LegalFollowups/Commands/App/* + CommitteeLegalFollowupsController — ميداني",
    "Admin LegalFollowupsController DownloadDocument — معاينة لوحة",
    "Application/ManagementTasks/* + ManagementTaskInboxRules — مهامي/انتقالات",
    "panel ManagementTasksPage inboxOnly + Management router sections",
]:
    doc.add_paragraph(line, style="List Bullet")

doc.add_heading("مسار البيانات 22", level=2)
doc.add_paragraph(
    "تطبيق → POST submit/{source} → Validate phone/branch/permission → lock(SubmissionKey) → "
    "SalesRequest New unassigned → Timeline Created + Outbox → لوحة Unassigned/Match."
)
doc.add_heading("انتقالات 23", level=2)
doc.add_paragraph(
    "Sent→Accept/Reject؛ Transfer→branch؛ Complete ذاتي→Completed؛ Complete بعد نقل→AwaitingDelegatedConfirm→Confirm→Completed؛ Resend من Returned."
)
doc.add_heading("نواقص", level=2)
doc.add_paragraph(
    "Device-verified لأزرار الإنشاء؛ إثبات concurrency حي إن لم تُشغَّل iso؛ نواقص 10–19 تبقى Open. لا 24 ولا نشر إنتاج."
)
doc.save(ROOT / "Unified_Admin_Delivery_22_23_Architecture.docx")
print("Wrote Unified_Admin_Delivery_22_23.xlsx and Architecture.docx")
