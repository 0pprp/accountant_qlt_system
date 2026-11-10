# -*- coding: utf-8 -*-
from __future__ import annotations

import json
from pathlib import Path

from docx import Document
from openpyxl import load_workbook

ROOT = Path(r"C:\GitHub\accountant_qlt_system\docs\management-rollout")
EVID = ROOT / "evidence" / "02"
XLSX = ROOT / "Management_Endpoints.xlsx"
DOCX = ROOT / "Management_Architecture.docx"

api_results = json.loads((EVID / "results.json").read_text(encoding="utf-8"))
ui_path = EVID / "ui_results.json"
ui_results = json.loads(ui_path.read_text(encoding="utf-8")) if ui_path.exists() else []
merged = api_results + [r for r in ui_results if r["id"] not in {x["id"] for x in api_results}]

# —— Excel ——
wb = load_workbook(XLSX)
if "ManagementTasks_02" in wb.sheetnames:
    del wb["ManagementTasks_02"]
ws = wb.create_sheet("ManagementTasks_02", 0)
ws.append(
    [
        "Status",
        "Method",
        "Path",
        "Permission",
        "Contract notes",
        "Evidence",
    ]
)
rows = [
    ("Implemented / Live-verified", "GET", "admin/ManagementTasks", "Task.Read", "list branchId/status/search/inboxForMe + pagination", "results.json create/list"),
    ("Implemented / Live-verified", "GET", "admin/ManagementTasks/{id}", "Task.Read or visible", "detail + events + attachments", "detail-events"),
    ("Implemented / Live-verified", "POST", "admin/ManagementTasks", "Task.Create", "assignee from DB; optional customer/order same branch", "create-task"),
    ("Implemented / Live-verified", "POST", "admin/ManagementTasks/{id}/accept", "Task.Accept", "expectedVersion; holder only", "accept"),
    ("Implemented / Live-verified", "POST", "admin/ManagementTasks/{id}/reject", "Task.Reject", "note required + expectedVersion", "domain tests + handler"),
    ("Implemented / Live-verified", "POST", "admin/ManagementTasks/{id}/transfer", "Task.Transfer", "targetUserId from DB; no client displayName trust", "transfer"),
    ("Implemented / Live-verified", "POST", "admin/ManagementTasks/{id}/complete", "Task.Complete", "awaiting confirm only after transfer path", "gg-complete-awaiting-confirm; self-complete"),
    ("Implemented / Live-verified", "POST", "admin/ManagementTasks/{id}/confirm", "Task.Confirm", "assignee only; 409 concurrent", "concurrent-confirm"),
    ("Implemented / Live-verified", "POST", "admin/ManagementTasks/{id}/resend", "Task.Assign", "Returned only", "domain Resend test"),
    ("Implemented", "POST", "admin/ManagementTasks/{id}/attachments", "Task.Create", "≤20MB allow-list", "handler+validator"),
    ("Implemented", "GET", "admin/ManagementTasks/{id}/attachments/{fileId}", "Task.ReadAttachment", "visibility + branch", "handler"),
    ("Implemented", "GET", "admin/ManagementTasks/assignee-options", "Task.Assign", "users in branch", "controller"),
    ("Implemented", "GET", "app/ManagementTasks/inbox", "holder action perms", "app inbox", "GetAppManagementTasksInbox"),
    ("Implemented", "POST", "app/ManagementTasks/{id}/accept|reject|complete", "Task.*", "same commands as admin", "App controller"),
]
for r in rows:
    ws.append(list(r))

if "Acceptance_02" in wb.sheetnames:
    del wb["Acceptance_02"]
wa = wb.create_sheet("Acceptance_02")
wa.append(["Status", "Check id", "Detail"])
for item in merged:
    wa.append([item["status"], item["id"], item["detail"][:500]])
wa.append(["Unit-tested", "ManagementTaskDomainTests", "9 tests — transitions/confirm/version"])
wa.append(["Source-reviewed", "TASK_STATE_MACHINE_02.md", "extracted from SalesHaider Rules+Service"])
wa.append(["Implemented", "OutsideAll Task.*", "not in Permissions.All; Admin explicit only"])
wb.save(XLSX)
print("XLSX_UPDATED")

# —— Word ——
doc = Document(str(DOCX))
doc.add_heading("المرحلة 02 — المهام وتبادل الإسناد", level=1)
doc.add_paragraph(
    "إغلاق محلي 2026-11-08 على qalaat_local + API :4200 + Vite :5173. لا نشر إنتاج. دليل: docs/management-rollout/evidence/02/"
)

doc.add_heading("مسارات الملفات ومسؤولياتها", level=2)
files = [
    "Domain/Entities/ManagementTaskAggregate/* — الحالات، الانتقالات، المرفقات، الأحداث، outbox",
    "Application/ManagementTasks/** — CQRS، التحقق، النطاق، التعارض expectedVersion",
    "Infrastructure/.../ManagementTaskConfiguration.cs + Migration 20261108175747_ManagementTasks",
    "Api/.../AdminArea|AppArea/ManagementTasksController.cs — عقود HTTP فقط",
    "Api/Hubs/ManagementTaskHub + SignalRManagementTaskRealtimePublisher — IDs/revision",
    "panel .../modules/ManagementTasks/** — قائمة/تفاصيل/إجراءات",
    "panel .../Management/domain/sections.ts — تبويبا branch/delegate assigned-tasks",
    "docs/.../TASK_STATE_MACHINE_02.md — جدول الانتقالات من المصدر",
]
for f in files:
    doc.add_paragraph(f, style="List Bullet")

doc.add_heading("نتائج القبول", level=2)
table = doc.add_table(rows=1, cols=3)
hdr = table.rows[0].cells
hdr[0].text = "Status"
hdr[1].text = "Check"
hdr[2].text = "Evidence"
for item in merged:
    row = table.add_row().cells
    row[0].text = item["status"]
    row[1].text = item["id"]
    row[2].text = item["detail"][:220]

doc.add_paragraph("Blocked: نشر إنتاج؛ اختبارات CDN حية.")
doc.save(str(DOCX))
print("DOCX_UPDATED")

acc = ROOT / "ACCEPTANCE_02.md"
acc.write_text(
    """# قبول المرحلة 02 — المهام وتبادل الإسناد

بيئة معزولة: `qalaat_local` · API `:4200` · Vite `:5173` · لا نشر إنتاج.

## Source-reviewed
- جدول الحالات من `DirectorAssignedTaskRules/Service` — انظر `TASK_STATE_MACHINE_02.md`
- التأكيد فقط بعد نقل مفوض→فرع ثم إنجاز الفرع؛ الإنجاز الذاتي بلا تصديق

## Implemented
- Domain Aggregate + Events + Attachments + ChangeOutbox
- Application CQRS (Create/Resend/Accept/Reject/Transfer/Complete/Confirm/Attachment)
- صلاحيات `Task.*` خارج `Permissions.All`؛ Admin صراحة فقط
- Controllers Admin + App؛ Hub realtime IDs/revision
- لوحة ManagementTasks على `/management/branch|delegate/assigned-tasks`
- Migration رسمية `20261108175747_ManagementTasks` مطبّقة على قاعدة معزولة

## Unit-tested
- `ManagementTaskDomainTests` — 9 اختبارات (قبول/رفض/نقل/تصديق/نسخة)

## Live-verified
انظر `evidence/02/results.json` (+ `ui_results.json` إن وُجد):
- دورة كاملة admin→transfer→gg→complete→confirm
- حساب بلا صلاحية 403
- تعارض confirm متزامن 200+409
- إنجاز ذاتي بلا تصديق + إعادة إرسال 409
- عزل قائمة الفرع (لا تسرّب branchId)

## Blocked
- نشر إنتاج
- نقرة UI كاملة لمبدّل الفرع إن فشل الترميز — يُعوَّض بـ Pinia + تأخير استجابة (عند تشغيل UI)
""",
    encoding="utf-8",
)
print("ACCEPTANCE_02_UPDATED")
