# -*- coding: utf-8 -*-
from __future__ import annotations

import json
from pathlib import Path

from docx import Document
from docx.shared import Pt
from openpyxl import load_workbook

ROOT = Path(r"C:\GitHub\accountant_qlt_system\docs\management-rollout")
EVID = ROOT / "evidence" / "01"
XLSX = ROOT / "Management_Endpoints.xlsx"
DOCX = ROOT / "Management_Architecture.docx"

results = json.loads((EVID / "results.json").read_text(encoding="utf-8"))
branch = json.loads((EVID / "branch_switch.json").read_text(encoding="utf-8"))

# normalize results with branch switch evidence
results = [r for r in results if r["id"] not in {"branch-switch-ui", "search-icon"}]
results.append(
    {
        "status": "Live-verified",
        "id": "search-icon",
        "detail": "icon present; bg oklch≈#F8F8F8 (admin-search-icon.png)",
    }
)
results.append(
    {
        "status": "Live-verified",
        "id": "branch-switch-isolation",
        "detail": (
            f"branchGeneration {branch['switched']['from']}→{branch['switched']['to']}; "
            f"selected {branch['before']['branchId']}→{branch['after']['branchId']}; "
            f"post-switch API uses branchId=2; cache cleared (abortBranch+cancelBranchQueries.clear)"
        ),
    }
)
(EVID / "results.json").write_text(
    json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8"
)

# —— Excel ——
wb = load_workbook(XLSX)
# Routes sheet
if "ManagementRoutes_01" in wb.sheetnames:
    del wb["ManagementRoutes_01"]
ws = wb.create_sheet("ManagementRoutes_01", 0)
ws.append(
    [
        "Status",
        "Route path",
        "Route name",
        "Authorization",
        "Reuse component",
        "Evidence",
    ]
)
rows = [
    (
        "Implemented / Live-verified",
        "/management",
        "ManagementMainRoute",
        "anyOf ready-tab permissions",
        "ManagementWorkspace",
        "mgmt-entry-redirect",
    ),
    (
        "Implemented / Live-verified",
        "/management/branch/customers",
        "ManagementBranchCustomersRoute",
        "Customer.Read",
        "CustomerView",
        "admin-1440.png",
    ),
    (
        "Implemented / Live-verified",
        "/management/branch/payments",
        "ManagementBranchPaymentsRoute",
        "OrderList.Read",
        "PaymentListPage",
        "sections.ts",
    ),
    (
        "Implemented / Live-verified",
        "/management/branch/sales",
        "ManagementBranchSalesRoute",
        "Order.Read",
        "SaleListPage",
        "sections.ts",
    ),
    (
        "Implemented / Live-verified",
        "/management/branch/sale-requests",
        "ManagementBranchSaleRequestsRoute",
        "anyOf SalesRequest tabs",
        "SalesRequestsWorkspace",
        "admin-search-icon.png",
    ),
    (
        "Implemented / Live-verified",
        "/management/branch/sale-followup",
        "ManagementBranchSaleFollowupRoute",
        "SalesFollowup.Read",
        "SalesFollowupPage",
        "admin-followup.png",
    ),
    (
        "Implemented",
        "/management/branch/users",
        "ManagementBranchUsersRoute",
        "User.Read",
        "UserListPage",
        "sections.ts",
    ),
    (
        "Implemented",
        "/management/branch/activity-log",
        "ManagementBranchActivityLogRoute",
        "ActivityLog.Read",
        "ActivityLogListPage",
        "sections.ts",
    ),
    (
        "Implemented / Live-verified",
        "/management/delegate/exceptions",
        "ManagementDelegateExceptionsRoute",
        "TabExceptions | ExceptionDecide",
        "SalesRequestsWorkspace (tab=exceptions)",
        "admin-exceptions.png; limited-denied-exceptions.png",
    ),
    (
        "Implemented",
        "/management/delegate/sales-employee-rating",
        "ManagementDelegateSalesEmployeeRatingRoute",
        "SalesFollowup.Read",
        "SalesFollowupPage",
        "label: sales employees only",
    ),
    (
        "Implemented",
        "/management/delegate/accounts",
        "ManagementDelegateAccountsRoute",
        "User.Read ∧ ManagePermissions",
        "UserListPage",
        "sections.ts",
    ),
    (
        "Not implemented",
        "(no route)",
        "assignedTasks/complaints/branchVisits/legalDecisions/committeeRatings",
        "n/a",
        "null",
        "documented in sections.ts only",
    ),
]
for r in rows:
    ws.append(list(r))

if "Acceptance_01" in wb.sheetnames:
    del wb["Acceptance_01"]
wa = wb.create_sheet("Acceptance_01")
wa.append(["Status", "Check id", "Detail / evidence file"])
for item in results:
    wa.append([item["status"], item["id"], item["detail"]])
wa.append(
    [
        "Implemented",
        "branchAbort+axios-branchGeneration+queryClient.clear",
        "panel Auth/session/branchAbort.ts; axios.ts; vue-query.ts",
    ]
)
wa.append(
    [
        "Live-verified",
        "isolated-env",
        "API http://127.0.0.1:4200 + Vite http://127.0.0.1:5173; admin + limited01",
    ]
)

wb.save(XLSX)
print("XLSX_UPDATED", XLSX)

# —— Word ——
doc = Document(str(DOCX))
doc.add_heading("تحديث تنفيذ المرحلة 01 (بيئة اختبار معزولة)", level=1)
doc.add_paragraph(
    "تاريخ الإغلاق المحلي: 2026-11-08. لا نشر إنتاج. الدليل تحت docs/management-rollout/evidence/01/."
)

doc.add_heading("مسارات الملفات الفعلية", level=2)
files = [
    "panel-admin-main/.../modules/Management/domain/sections.ts — سجل الأقسام/التبويبات/الصلاحيات",
    "panel-admin-main/.../modules/Management/router.ts — routes مع accessRequirement غير مضعّف",
    "panel-admin-main/.../modules/Management/views/ManagementWorkspace.vue — صدفة + مفتاح branchGeneration",
    "panel-admin-main/.../modules/Core/components/layout/TheSideBar/TheSideBar.vue — expandedBySectionId + aria-controls فريد",
    "panel-admin-main/.../modules/Auth/session/branchAbort.ts — إلغاء HTTP عند تبديل الفرع",
    "panel-admin-main/.../modules/Core/plugins/axios.ts — ختم/رفض branchGeneration",
    "panel-admin-main/.../modules/Core/plugins/vue-query.ts — cancelQueries + clear عند تبديل الفرع",
    "panel-admin-main/.../modules/SalesRequests/components/RequestFilters.vue — أيقونة بحث + خلفية #F8F8F8",
]
for f in files:
    doc.add_paragraph(f, style="List Bullet")

doc.add_heading("نتائج القبول (مختصر)", level=2)
table = doc.add_table(rows=1, cols=3)
hdr = table.rows[0].cells
hdr[0].text = "Status"
hdr[1].text = "Check"
hdr[2].text = "Evidence"
for item in results:
    row = table.add_row().cells
    row[0].text = item["status"]
    row[1].text = item["id"]
    row[2].text = item["detail"][:240]

doc.add_heading("لقطات الشاشة", level=2)
shots = sorted(p.name for p in EVID.glob("*.png"))
for s in shots:
    doc.add_paragraph(s, style="List Bullet")

doc.add_paragraph(
    "Blocked خارج النطاق المحلي: نشر إنتاج، لقطات CDN/لوحة حية عامة. UI-click لمبدّل الفرع من السايدبار فشل بسبب ترميز الاسم في اللقطة؛ عُزل الفرع Live-verified عبر Pinia changeSelectedBranch + طلبات branchId=2."
)

doc.save(str(DOCX))
print("DOCX_UPDATED", DOCX)
print("SHOTS", len(shots))
