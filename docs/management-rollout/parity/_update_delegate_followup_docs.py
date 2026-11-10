# -*- coding: utf-8 -*-
import json
from pathlib import Path

from docx import Document
from openpyxl import load_workbook

ROOT = Path(r"C:\GitHub\accountant_qlt_system\docs\management-rollout")
PARITY = ROOT / "parity"
EVID = ROOT / "evidence" / "03"
XLSX = ROOT / "Management_Endpoints.xlsx"
DOCX = ROOT / "Management_Architecture.docx"
PXLSX = PARITY / "WorkflowParity.xlsx"

api = json.loads((EVID / "results.json").read_text(encoding="utf-8"))
ui = json.loads((EVID / "ui_results.json").read_text(encoding="utf-8")) if (EVID / "ui_results.json").exists() else []
merged = api + ui

wb = load_workbook(XLSX)
if "DelegateFollowup_01" in wb.sheetnames:
    del wb["DelegateFollowup_01"]
ws = wb.create_sheet("DelegateFollowup_01", 0)
ws.append(["Status", "Method", "Path", "Permission", "Notes", "Evidence"])
for row in [
    ("Implemented / Live-verified", "GET", "admin/DelegateFollowup/lists?branchId=", "DelegateFollowup.Read", "active OrderLists", "lists"),
    ("Implemented / Live-verified", "GET", "admin/DelegateFollowup/lists/{id}/summary?branchId=", "DelegateFollowup.Read", "honest empty committees/legal", "legal-unavailable-honest"),
    ("Implemented / Live-verified", "GET", "admin/DelegateFollowup/lists/{id}/customers?branchId=", "DelegateFollowup.Read", "open-balance customers", "customers"),
    ("Implemented / Live-verified", "GET/POST", "admin/DelegateFollowup/lists/{id}/board-operator-rating", "Read / UpsertBoardRating", "daily MERGE 0-10", "board-rating-upsert"),
    ("Independent", "GET", "admin/SalesFollowup/employees", "SalesFollowup.Read", "untouched", "salesfollowup-untouched"),
]:
    ws.append(list(row))

if "Acceptance_DF_01" in wb.sheetnames:
    del wb["Acceptance_DF_01"]
wa = wb.create_sheet("Acceptance_DF_01")
wa.append(["Status", "Check", "Detail"])
for item in merged:
    wa.append([item["status"], item["id"], str(item["detail"])[:500]])
wa.append(["Unit-tested", "DelegateFollowupRulesTests", "6 tests"])
wa.append(["Source-reviewed", "DirectorDelegateFollowupRules/Service", "parity docs"])
wb.save(XLSX)
print("XLSX_OK")

# parity workbook
pwb = load_workbook(PXLSX)
if "DF_Coverage" in pwb.sheetnames:
    del pwb["DF_Coverage"]
pc = pwb.create_sheet("DF_Coverage", 0)
pc.append(["Item", "Status", "Evidence"])
for line in (PARITY / "Coverage_DelegateFollowup_01.md").read_text(encoding="utf-8").splitlines():
    if line.startswith("|") and "البند" not in line and "---" not in line:
        cells = [c.strip() for c in line.strip("|").split("|")]
        if len(cells) >= 3:
            pc.append(cells[:3])
pwb.save(PXLSX)
print("PARITY_XLSX_OK")

doc = Document(str(DOCX))
doc.add_heading("متابعة المندوبين — تنفيذ برومبت 01", level=1)
doc.add_paragraph(
    "مسار اللوحة: /management/director/delegate-followup. "
    "صلاحيات DelegateFollowup.* خارج Permissions.All. "
    "SalesFollowup وManagementTasks مستقلان بلا حذف."
)
doc.add_heading("ملفات فعلية", level=2)
for f in [
    "Application/DelegateFollowup/** — قواعد، تجميع، استعلامات، تقييم عامل اللوحة",
    "Domain/.../DirectorBoardOperatorListRating.cs",
    "Api/.../AdminArea/DelegateFollowupController.cs",
    "Migration 20261108183215_DelegateFollowupBoardOperatorRating",
    "panel modules/DelegateFollowup/** — لوحة كاملة",
    "Management/domain/sections.ts — قسم director + فك التباس التسميات",
    "parity/Coverage_DelegateFollowup_01.md — مصفوفة التغطية",
    "evidence/03/ — نتائج ولقطات",
]:
    doc.add_paragraph(f, style="List Bullet")
doc.add_heading("غير محسوم", level=2)
doc.add_paragraph("IsLegal، جداول تقييم اللجان للقائمة، قسط مطلوب بلا استثناء قانونية.")
doc.save(str(DOCX))
print("DOCX_OK")
