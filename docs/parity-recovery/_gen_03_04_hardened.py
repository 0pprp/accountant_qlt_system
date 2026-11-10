# -*- coding: utf-8 -*-
from pathlib import Path
from openpyxl import load_workbook
from openpyxl.styles import Font, PatternFill
from docx import Document
from docx.shared import Pt
from datetime import datetime

root = Path(__file__).resolve().parent
wb = load_workbook(root / "Parity_Matrix.xlsx")
fill = PatternFill("solid", fgColor="006A64")
hdr = Font(bold=True, color="FFFFFF")

name = "Eligibility"
if name in wb.sheetnames:
    del wb[name]
ws = wb.create_sheet(name)
ws.append(["Rule", "AppliesTo", "Source", "Notes"])
for r in [
    (
        "Active SalesEmployeeAssignment on request branch at now",
        "assign + transfer-peers + transfer-name",
        "SalesWorkforce.ActiveAssignments",
        "not Mandob role alone",
    ),
    (
        "Completed account + username + password hash",
        "same",
        "Users",
        "valid account",
    ),
    (
        "Effective SalesRequest.AppRead minus Deny",
        "same",
        "RolePermissions|UserPermissions - Denials",
        "Deny wins",
    ),
    (
        "Same QueryEligibleAssignees",
        "admin assignable + app peers + transfer",
        "SalesRequestEmployeeEligibility",
        "unified",
    ),
]:
    ws.append(list(r))
for c in ws[1]:
    c.font = hdr
    c.fill = fill

if "Acceptance" in wb.sheetnames:
    ws = wb["Acceptance"]
    for row in [
        ["Unified eligibility SalesWorkforce+AppRead", "Y", "Y", "Y assignable A/B only", "N", "partial", "bm_iso excluded"],
        ["ensure-draft advisory lock PG", "Y", "Y", "Y same LinkedOrderId", "N", "N", "no orphan on reassign race"],
        ["Transfer A→B lockout A", "Y", "Y", "Y api-live-ab-transfer.log", "N", "partial Device", "Not_Owner after"],
        ["Complaint sales-employee source", "Y", "Y", "Y sourceApp=sales-employee", "N", "N", "not AppNote"],
        ["AppNote SalesEmployee source", "Y", "Y", "Y sourceApp=SalesEmployee", "N", "N", "assignment precedes Mandob→Delegate"],
    ]:
        ws.append(row)

if "OpenGaps" in wb.sheetnames:
    ws = wb["OpenGaps"]
    ws.append(["Complaint sales employee (resolved)", "—", "POST app/Complaints", "Mandob Complaint.Submit + assignment"])
    ws.append(["AppNote Mandob misclassify (resolved)", "—", "SalesEmployee when assignment active", "AppNoteSourceResolver"])

wb.save(root / "Parity_Matrix.xlsx")

doc = Document()
doc.styles["Normal"].font.name = "Calibri"
doc.styles["Normal"].font.size = Pt(11)
doc.add_heading("Parity Recovery — Architecture (03–04 hardened)", level=1)
doc.add_paragraph(
    f"Updated {datetime.now().isoformat(timespec='seconds')} — eligibility+ensure-draft+complaint/note. No production deploy."
)
doc.add_heading("Endpoints (delta)", level=2)
doc.add_paragraph(
    "NEW GET /api/v1/app/SalesRequests/{id}/transfer-peers — AppTransferName — QueryEligibleAssignees (assignment+AppRead)."
)
doc.add_paragraph(
    "NEW POST /api/v1/app/SalesRequests/{id}/transfer-name — same eligibility; locks previous owner."
)
doc.add_paragraph(
    "POST /api/v1/app/SalesRequests/{id}/ensure-draft — pg_advisory_xact_lock(791901,id) + ownership/status recheck before Order; rollback orphans."
)
doc.add_paragraph(
    "NEW POST /api/v1/app/Complaints — Complaint.Submit; server source sales-employee when active assignment (not AppNote)."
)
doc.add_paragraph(
    "POST /api/v1/app/AppNotes — source resolved server-side: SalesEmployee if assignment else role map."
)
doc.add_heading("FileMap", level=2)
doc.add_paragraph(
    "SalesRequestEmployeeEligibility.cs, SalesWorkforce.cs, EnsureSalesRequestDraftHandler.cs, AppNoteSourceResolver.cs, AppArea/ComplaintsController.cs, SubmitComplaintPage.dart"
)
doc.add_heading("Acceptance / OpenGaps", level=2)
doc.add_paragraph(
    "See Parity_Matrix.xlsx. Device remains partial (screencap black; do not disable FLAG_SECURE)."
)
doc.add_heading("Intended security differences", level=2)
doc.add_paragraph(
    "Assignee eligibility requires workforce assignment + AppRead; Mandob alone insufficient. AppNote/Complaint sources from assignment, not role name alone."
)
doc.save(root / "Architecture.docx")
print("updated matrix+architecture")
