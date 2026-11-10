from pathlib import Path
from datetime import datetime

import openpyxl
from docx import Document

root = Path(r"C:\GitHub\accountant_qlt_system\docs\parity-recovery")
xlsx = root / "Parity_Matrix.xlsx"
docx_path = root / "Architecture.docx"

wb = openpyxl.load_workbook(xlsx)
# Prefer first sheet or create Updates
name = "05_06_Updates"
if name in wb.sheetnames:
    ws = wb[name]
    ws.delete_rows(1, ws.max_row)
else:
    ws = wb.create_sheet(name)

headers = [
    "Batch",
    "ScreenOrFlow",
    "LegacyRef",
    "NewAPI",
    "Implemented",
    "Unit",
    "API_live",
    "Browser_live",
    "Device",
    "Notes",
]
ws.append(headers)
rows = [
    [
        "05",
        "FieldFollowups list/add/complete/record-complete/latest-notes",
        "Followers/FieldFollowups*",
        "GET|POST /api/v1/app/committee/field-followups*",
        "Y",
        "Y",
        "Y",
        "N",
        "Partial",
        "Not LegalFollowup/ManagementTask; Motaba scope",
    ],
    [
        "05",
        "ListStats drawer",
        "Followers/ListStats",
        "GET /api/v1/app/committee/list-stats",
        "Y",
        "N",
        "Y",
        "N",
        "Partial",
        "PreviousMonthAvailable=false intentional",
    ],
    [
        "05",
        "CustomerNote / DelegateNote / DirectorNote",
        "Follower notes triad",
        "committee customers/notes + delegate-notes + director-notes",
        "Y",
        "N",
        "Y",
        "N",
        "Partial",
        "HasNote on daily after CustomerNote",
    ],
    [
        "06",
        "Collect/sync ClientPaymentId idempotency",
        "CustomersPayments ClientPaymentId",
        "POST Orders/{id}/installment-payments + Sync (ClientPaymentId NEW)",
        "Y",
        "Y",
        "Y",
        "N",
        "Partial",
        "KEY_COUNT=1 on retry; NEW contract documented",
    ],
    [
        "06",
        "FollowerCollectionEnabled list lock",
        "login/refresh + list flag",
        "profile.orderList.followerCollectionEnabled + MandobLocked",
        "Y",
        "Y",
        "Y",
        "N",
        "Partial",
        "List-scoped; not account-wide",
    ],
    [
        "06",
        "First collection eval / WrongSale",
        "FirstCollectionEvaluation",
        "POST /api/v1/app/WrongSales/first-collection-eval",
        "Y",
        "Y",
        "N",
        "N",
        "Partial",
        "UI blocks collect if eval fails",
    ],
    [
        "03-04",
        "Open gaps retained",
        "existing-customer; Device black",
        "—",
        "—",
        "—",
        "—",
        "—",
        "Partial",
        "Kept in Progress OpenGaps",
    ],
]
for r in rows:
    ws.append(r)
wb.save(xlsx)

doc = Document(docx_path)
doc.add_heading("Parity recovery 05–06 — Endpoints & FileMap", level=1)
doc.add_paragraph(f"Updated {datetime.utcnow().isoformat()}Z — isolated DB only; no production.")

doc.add_heading("Endpoints (05)", level=2)
for line in [
    "GET /api/v1/app/committee/field-followups — auth JWT OrderList.Read — Motaba pending rows",
    "POST /api/v1/app/committee/field-followups — body listId,customerId,... — idempotent pending",
    "POST /api/v1/app/committee/field-followups/{id}/complete — body note (required)",
    "POST /api/v1/app/committee/field-followups/record-complete — add+complete",
    "GET /api/v1/app/committee/field-followups/latest-notes?listId=",
    "GET /api/v1/app/committee/list-stats — ListStats formulas; PreviousMonthAvailable flag",
]:
    doc.add_paragraph(line, style="List Bullet")

doc.add_heading("Endpoints (06)", level=2)
for line in [
    "POST /api/v1/app/Orders/{id}/installment-payments — body amount, clientPaymentId? (NEW optional) — idempotent ack",
    "POST /api/v1/app/InstallmentPayments/sync — body installmentPayments[].clientPaymentId — MandobCollectionLocked when enabled",
    "GET /api/v1/app/Users/profile — orderList.followerCollectionEnabled",
    "POST /api/v1/app/WrongSales/first-collection-eval — existing Mandob first-rating path",
]:
    doc.add_paragraph(line, style="List Bullet")

doc.add_heading("FileMap", level=2)
for line in [
    "Domain: FollowerFieldFollowup.cs; InstallmentPayment.ClientPaymentId",
    "Application: Committee FieldFollowups commands/queries; CommitteeListStatsBuilder; Create/Sync installment handlers",
    "Api: CommitteeController field-followups + list-stats; OrdersController ClientPaymentId pass-through",
    "Flutter committee: field_followups_page, list_stats_page, list_detail actions, app_drawer",
    "Flutter delegate: payments_remote clientPaymentId; queue owner isolation; collect lock dialog; session flag",
    "Migrations: 20261109230458_FollowerFieldFollowups; 20261109230700_InstallmentPaymentClientPaymentId",
]:
    doc.add_paragraph(line, style="List Bullet")

doc.add_heading("Acceptance / OpenGaps", level=2)
doc.add_paragraph(
    "05/06 Implemented+Unit+API-live on iso. Device Partial (lockscreen/black screencap). "
    "Browser-live widths pending. ListStats previous-month snapshots intentional gap. "
    "03–04 existing-customer + Device gaps retained. No batch 07. No production publish."
)

doc.save(docx_path)
print("UPDATED", xlsx, docx_path)
