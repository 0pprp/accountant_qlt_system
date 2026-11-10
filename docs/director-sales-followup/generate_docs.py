# -*- coding: utf-8 -*-
"""Generate Excel API map + Word architecture docs for SalesFollowup."""
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment, Border, Side
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Pt

ROOT = Path(__file__).resolve().parent
REPO = Path(__file__).resolve().parents[2]
ARCH_DIR = ROOT / "architecture" / "files"

BACKEND_ROOTS = [
    REPO / "api-main" / "api-main" / "src" / "Application" / "SalesFollowup",
    REPO / "api-main" / "api-main" / "src" / "Domain" / "Entities" / "SalesFollowupAggregate",
    REPO / "api-main" / "api-main" / "src" / "Api" / "Controllers" / "V1" / "AdminArea",
]
FRONTEND_ROOT = REPO / "panel-admin-main" / "panel-admin-main" / "src" / "modules" / "SalesFollowup"

ENDPOINTS = [
    ("GET", "api/v1/admin/SalesFollowup/employees", "SalesFollowup.Read", "branchId", "{items[],marker}", "كروت الموظفين دفعة واحدة", "Admin", "SalesFollowupBoard.vue", "نعم"),
    ("GET", "api/v1/admin/SalesFollowup/employees/{id}", "SalesFollowup.Read", "branchId", "DetailDto كامل+زبائن", "تفصيل مجمّع بلا N+1", "Admin", "useSalesFollowupDetailQuery.ts", "نعم"),
    ("GET", "api/v1/admin/SalesFollowup/employees/{id}/app-block", "SalesFollowup.Read", "branchId", "{isBlocked,...}", "حالة إيقاف التطبيق", "Admin", "SalesFollowupBoard.vue", "نعم"),
    ("PUT", "api/v1/admin/SalesFollowup/employees/{id}/app-block", "SalesFollowup.ManageAppBlock", "{isBlocked}", "{isBlocked,...}", "إيقاف/تفعيل التطبيق", "Admin", "useSetAppBlockMutation.ts", "نعم"),
    ("POST", "api/v1/admin/SalesFollowup/employees/{id}/manager-rating", "SalesFollowup.UpsertManagerRating", "{score,note}", "ManagerRatingDto", "تقييم المدير", "Admin", "—", "نعم"),
]


def write_excel():
    wb = Workbook()
    ws = wb.active
    ws.title = "API"
    headers = ["Method", "Path", "Auth", "Request", "Response", "Purpose", "Area", "ClientFile", "Accepted"]
    ws.append(headers)
    header_fill = PatternFill("solid", fgColor="006A64")
    header_font = Font(color="FFFFFF", bold=True)
    for col, _ in enumerate(headers, 1):
        cell = ws.cell(1, col)
        cell.fill = header_fill
        cell.font = header_font
        cell.alignment = Alignment(horizontal="center", wrap_text=True)
    for row in ENDPOINTS:
        ws.append(list(row))
    for col in ws.columns:
        width = max(len(str(c.value or "")) for c in col) + 2
        ws.column_dimensions[col[0].column_letter].width = min(width, 48)
    thin = Border(
        left=Side(style="thin"),
        right=Side(style="thin"),
        top=Side(style="thin"),
        bottom=Side(style="thin"),
    )
    for row in ws.iter_rows(min_row=1, max_row=ws.max_row, max_col=len(headers)):
        for cell in row:
            cell.border = thin
            cell.alignment = Alignment(wrap_text=True, vertical="top")
    wb.save(ROOT / "SalesFollowup_API_Documentation.xlsx")


FILE_ROLES = {
    "SalesFollowupController.cs": "AdminArea HTTP endpoints — صلاحيات + MediatR فقط.",
    "SalesFollowupRules.cs": "قواعد Low/High/Zeroed/Anonymize + DeployMarker.",
    "SalesFollowupDataService.cs": "تجميع موظفين/تفصيل/زبائن/تحويلات دفعة واحدة.",
    "SalesFollowupDtos.cs": "عقود الاستجابة للوحة.",
    "SalesFollowupAccess.cs": "نطاق الفرع عبر BranchIds.",
    "SalesFollowupErrors.cs": "أخطاء المجال.",
    "SalesEmployeeAppBlock.cs": "كيان إيقاف تطبيق الموظف.",
    "DirectorSalesEmployeeBoardRating.cs": "كيان تقييم المدير.",
    "SalesFollowupBoard.vue": "واجهة تبويب المبيعات داخل طلبات البيع.",
    "endpoints.ts": "ثوابت مسارات الـ API.",
    "types.ts": "أنواع TypeScript للعقود.",
    "useSalesFollowupEmployeesQuery.ts": "طلب قائمة الموظفين (1 request/فرع).",
    "useSalesFollowupDetailQuery.ts": "طلب التفصيل المجمّع + cache.",
    "useSetAppBlockMutation.ts": "إيقاف/تفعيل التطبيق.",
}


def collect_files():
    files = []
    for root in BACKEND_ROOTS:
        if not root.exists():
            continue
        if root.name == "AdminArea":
            for p in root.glob("SalesFollowupController.cs"):
                files.append(p)
            continue
        for p in root.rglob("*.cs"):
            files.append(p)
    if FRONTEND_ROOT.exists():
        for p in FRONTEND_ROOT.rglob("*.{ts,vue,json}".replace("{ts,vue,json}", "*")):
            if p.suffix in {".ts", ".vue", ".json"}:
                files.append(p)
    return sorted(files, key=lambda p: str(p))


def write_clean_guide():
    doc = Document()
    title = doc.add_heading("Sales Followup — Clean Architecture", 0)
    title.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    doc.add_paragraph(
        "Backend: Api Controllers → Application (Queries/Commands + DataService/Rules) → Domain Entities → Infrastructure EF. "
        "Frontend: modules/SalesFollowup داخل تبويب المبيعات تحت SalesRequests."
    )
    for layer, desc in [
        ("Domain/SalesFollowupAggregate", "AppBlock + ManagerBoardRating"),
        ("Application/SalesFollowup", "MediatR handlers، قواعد، تجميع بيانات"),
        ("Api/AdminArea/SalesFollowupController", "Endpoints محمية بصلاحيات"),
        ("panel SalesFollowup", "Board + queries/mutations + types"),
        ("SalesRequests tab=sales", "موضع التبويب الفرعي «المبيعات»"),
    ]:
        doc.add_heading(layer, level=1)
        p = doc.add_paragraph(desc)
        for run in p.runs:
            run.font.size = Pt(12)
    doc.save(ROOT / "Clean_Architecture_File_Guide.docx")


def write_file_docs():
    ARCH_DIR.mkdir(parents=True, exist_ok=True)
    files = collect_files()
    index = Document()
    index.add_heading("00_INDEX — SalesFollowup files", 0)
    for path in files:
        try:
            rel = path.relative_to(REPO).as_posix()
        except ValueError:
            rel = path.name
        index.add_paragraph(rel)
        safe = rel.replace("/", "__").replace("\\", "__")
        for ch in ". ":
            safe = safe.replace(ch, "_")
        safe = safe[:180] + ".docx"
        d = Document()
        d.add_heading(rel, 0)
        d.add_paragraph(f"مسار: {rel}")
        role = FILE_ROLES.get(path.name, "جزء من ميزة متابعة المبيعات (SalesFollowup).")
        d.add_paragraph(f"الدور: {role}")
        d.add_paragraph("الارتباطات: Backend عبر MediatR/EF؛ Frontend عبر axios + TanStack Query إلى admin/SalesFollowup.")
        d.save(ARCH_DIR / safe)
    index.save(ROOT / "architecture" / "00_INDEX.docx")


def main():
    write_excel()
    write_clean_guide()
    write_file_docs()
    print("Generated Excel + Word docs under", ROOT)


if __name__ == "__main__":
    main()
