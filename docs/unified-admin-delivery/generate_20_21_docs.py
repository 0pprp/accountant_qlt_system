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
ws.title = "20_Summary"
style_header(ws, ["Endpoint", "Permission", "Rule", "Status"])
append_rows(ws, [
    ("GET /admin/SalesSummary/matrix", "SalesSummary.Read", "salePrice only; SaleDate calendar + Baghdad 00:00 fallback", "API-live"),
    ("GET /admin/SalesSummary/details", "SalesSummary.Drilldown", "page totals = filtered cell; Drilldown ≠ Read", "API-live"),
    ("Metric other than salePrice", "—", "Rejected 400", "API-live"),
    ("≠ ManagementSummary", "independent OutsideAll", "No box cards / multi-metric", "Documented"),
    ("Admin RolePermissions seed", "SalesSummary.OutsideAll on Admin", "Fixed missing concat", "Implemented"),
])

ws2 = wb.create_sheet("21_Legal")
style_header(ws2, ["Transition", "Permission", "Effect", "Status"])
append_rows(ws2, [
    ("Create from New/Rejected", "LegalFollowup.Create", "Case InProgress; Request LegalFollowup; Outbox", "API-live"),
    ("List/Get", "LegalFollowup.Read", "Branch scoped", "API-live"),
    ("Cancel InProgress", "LegalFollowup.Cancel", "Case Cancelled; Request→New; no debt/customer delete", "API-live"),
    ("Cancel already Cancelled", "Cancel", "Idempotent no double effect", "API-live"),
    ("Tab legal", "SalesRequest.TabLegal", "Extra view ready", "Unit"),
    ("Case documents preview", "—", "Storage gap", "Missing"),
])

ws3 = wb.create_sheet("Tests")
style_header(ws3, ["Suite", "Layer", "Count/Result"])
append_rows(ws3, [
    ("SalesSummary* + LegalFollowup*", "API Unit", "18 passed"),
    ("nav + TabLegal + FE contracts", "FE Unit", "20 passed"),
    ("dotnet build Api", "Build", "0 errors"),
    ("vue-tsc --noEmit", "Type-check", "exit 0"),
    ("API-live qalaat_20_21_iso", "Live", "PASS"),
    ("Browser-live 360/768/1440", "Live", "PASS"),
])

ws4 = wb.create_sheet("CarryGaps")
style_header(ws4, ["Gap", "Batch", "Status"])
append_rows(ws4, [
    ("10–19 prior live gaps where documented", "10-19", "Open (not closed without proof)"),
    ("16 Followup Excel", "16", "Open"),
    ("Case document file preview", "21", "Missing"),
    ("22", "—", "Not started"),
])

wb.save(ROOT / "Unified_Admin_Delivery_20_21.xlsx")

doc = Document()
doc.add_heading("20+21 — ملخص المبيعات + المتابعة القانونية", level=1)
doc.add_paragraph(
    "20 يعيد استخدام قاعدة SalePrice من ManagementSummary عبر endpoint وصلاحيات مستقلة. "
    "حدود اليوم: SaleDate تقويمي مع fallback CreatedAt بحدود بغداد 00:00 — ليس قطع بيع 03:00. "
    "21 يكمل دورة LegalFollowupCases: إنشاء من طلب البيع، قائمة/تفاصيل، إلغاء مع Outbox وإعادة الطلب لغير مسند."
)
doc.add_heading("ملفات", level=2)
for line in [
    "Application/SalesSummary/* + Api/.../SalesSummaryController.cs",
    "panel SalesSummary/ (/sale/summary)",
    "Application/LegalFollowups Create/List/Get/Cancel + DataService",
    "LegalFollowupsController",
    "panel SalesLegal/ (/sale/legal) + Create من RequestLifecycleActions",
    "RolePermissions: SalesSummary.OutsideAll على Admin",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.add_heading("انتقالات قانونية", level=2)
doc.add_paragraph("New/Rejected → Create → InProgress + Request LegalFollowup")
doc.add_paragraph("InProgress → Cancel → Cancelled + Request New (idempotent إن أُعيد)")
doc.add_paragraph("لا حذف زبون/دين؛ Outbox StatusChanged مع نجاح المعاملة")
doc.add_heading("تحقق حي", level=2)
doc.add_paragraph("قاعدة معزولة qalaat_20_21_iso؛ API :4200؛ لوحة :5173؛ لقطات evidence/20 و evidence/21.")
doc.save(ROOT / "Unified_Admin_Delivery_20_21_Architecture.docx")
print("ok")
