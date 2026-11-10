# -*- coding: utf-8 -*-
"""Generate Excel API map + Word architecture docs for sales_employee_app."""
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment, Border, Side
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Pt

ROOT = Path(__file__).resolve().parent
APP_LIB = Path(__file__).resolve().parents[2] / "sales_employee_app" / "lib"
ARCH_DIR = ROOT / "architecture" / "files"

ENDPOINTS = [
    ("POST", "api/v1/app/Auth/login", "لا", "{username,password}", "{accessToken,refreshToken}", "دخول JWT", "Auth", "features/auth/data/auth_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/Auth/refresh-token", "Refresh", "{refreshToken}", "{accessToken,refreshToken}", "تجديد الجلسة", "Auth", "features/auth/data/auth_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/Auth/logout", "Bearer", "{refreshToken}", "204", "خروج", "Auth", "features/auth/data/auth_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/Users/profile", "Bearer", "—", "ملف المستخدم", "اسم العرض", "Users", "features/auth/data/auth_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/Customers", "Bearer", "search,take", "قائمة زبائن", "بحث زبائن", "Customers", "features/customers/data/customers_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/Customers/{id}", "Bearer", "—", "زبون", "ملف مختصر", "Customers", "features/customers/data/customers_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/Products", "Bearer", "search", "منتجات", "كتالوج", "Products", "features/catalog/data/catalog_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/Orders", "Bearer", "filter", "طلبات", "سجل طلبات", "Orders", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/Orders/{id}", "Bearer", "—", "طلب", "تفاصيل", "Orders", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/Orders", "Bearer", "{customerId,orderItems}", "{id}", "إنشاء مبيع", "Orders", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("PUT", "api/v1/app/Orders/{id}/seller-info", "Bearer", "address,location,date,time", "نجاح", "معلومات البائع", "Orders", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/Orders/{id}/complete-attachments", "Bearer", "—", "نجاح", "إكمال المرفقات", "Orders", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/Attachments", "Bearer", "multipart File,OrderId,Type", "مرفق", "رفع مستند", "Attachments", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/App/checkVersion", "لا", "buildNumber", "{shouldUpdate,isForce,url}", "بوابة تحديث", "App", "features/updates/data/app_version_datasource.dart", "نعم"),
    ("POST", "api/v1/app/SalesShifts/start", "Bearer", "lat,lng", "وردية", "بدء وردية", "SalesShifts", "features/shift/data/shift_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/SalesShifts/end", "Bearer", "lat,lng", "نجاح", "إنهاء وردية", "SalesShifts", "features/shift/data/shift_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/SalesShifts/current", "Bearer", "—", "وردية حالية", "حالة الوردية", "SalesShifts", "features/shift/data/shift_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/SalesLocations/batch", "Bearer", "{points[]}", "نجاح", "طابور GPS", "SalesLocations", "features/shift/data/location_queue_datasource.dart", "نعم"),
    ("POST", "api/v1/app/SalesLocations/live", "Bearer", "{lat,lng}", "نجاح", "موقع حي", "SalesLocations", "features/shift/data/location_queue_datasource.dart", "نعم"),
    ("GET", "api/v1/app/SalesWorkflow/bins", "Bearer", "—", "صناديق حالات", "عدادات", "SalesWorkflow", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/SalesWorkflow/by-status", "Bearer", "status", "قائمة", "صندوق حالة", "SalesWorkflow", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/SalesWorkflow/{id}/progress", "Bearer", "—", "عنصر", "تقدم حالة", "SalesWorkflow", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("POST", "api/v1/app/SalesWorkflow/{id}/complete", "Bearer", "—", "عنصر", "إكمال بيع", "SalesWorkflow", "features/sales/data/sales_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/SalesInventory/inventory", "Bearer", "—", "مخزون", "مستودع", "SalesInventory", "features/catalog/data/catalog_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/SalesPriceBoard/price-board", "Bearer", "—", "أسعار", "لوحة أسعار", "SalesPriceBoard", "features/catalog/data/catalog_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/SalesRatings/summary", "Bearer", "—", "ملخص مستويات", "تقييمات", "SalesRatings", "features/ratings/data/ratings_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/SalesRatings/by-level", "Bearer", "level", "زبائن", "حسب المستوى", "SalesRatings", "features/ratings/data/ratings_remote_datasource.dart", "نعم"),
    ("GET", "api/v1/app/SalesRatings/customer/{id}/profile-lite", "Bearer", "—", "ملف مختصر", "تفاصيل تقييم", "SalesRatings", "features/ratings/data/ratings_remote_datasource.dart", "نعم"),
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
    wb.save(ROOT / "SalesEmployee_API_Documentation.xlsx")


def write_clean_guide():
    doc = Document()
    title = doc.add_heading("Sales Employee App — Clean Architecture", 0)
    title.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    doc.add_paragraph(
        "الطبقات: presentation → domain/data → core (theme, dio, session, database). "
        "الاعتماديات عبر Injection اليدوي. الـ API على api.qalataldhaman.com AppArea."
    )
    for layer, desc in [
        ("core", "ثيم قلعة، Dio+JWT refresh، SecureStorage، SQLite لطابور GPS"),
        ("features/auth", "دخول بدون prefills"),
        ("features/home", "Shell بثلاث تبويبات + اختصارات"),
        ("features/shift", "وردية + طابور مواقع أوفلاين"),
        ("features/sales", "صناديق حالات، معالج بيع، تفاصيل، نجاح بصوت"),
        ("features/customers", "بحث زبائن"),
        ("features/catalog", "مستودع + لوحة أسعار"),
        ("features/ratings", "ملخص وتقييمات زبائني"),
        ("features/updates", "Splash + بوابة تحديث"),
    ]:
        doc.add_heading(layer, level=1)
        p = doc.add_paragraph(desc)
        for run in p.runs:
            run.font.size = Pt(12)
    doc.save(ROOT / "Clean_Architecture_File_Guide.docx")


def dart_files():
    if not APP_LIB.exists():
        return []
    return sorted(p.relative_to(APP_LIB).as_posix() for p in APP_LIB.rglob("*.dart"))


def write_file_docs():
    ARCH_DIR.mkdir(parents=True, exist_ok=True)
    files = dart_files()
    index = Document()
    index.add_heading("00_INDEX — sales_employee_app/lib", 0)
    for rel in files:
        index.add_paragraph(rel)
        safe = rel.replace("/", "__").replace(".dart", "") + ".docx"
        d = Document()
        d.add_heading(rel, 0)
        d.add_paragraph(f"مسار: lib/{rel}")
        d.add_paragraph("الدور: جزء من تطبيق موظف المبيعات (قلعة الضمان).")
        d.add_paragraph("الارتباطات: عبر Injection وواجهات الـ feature؛ الشبكة عبر DioClient إلى api-main AppArea.")
        d.save(ARCH_DIR / safe)
    index.save(ROOT / "architecture" / "00_INDEX.docx")


def main():
    write_excel()
    write_clean_guide()
    write_file_docs()
    print("Generated Excel + Word docs under", ROOT)


if __name__ == "__main__":
    main()
