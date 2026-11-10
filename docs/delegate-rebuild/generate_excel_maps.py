# -*- coding: utf-8 -*-
"""Generate Excel API registry (3 cols) + Clean Architecture map for delegate_app."""
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment, Border, Side
from openpyxl.utils import get_column_letter

ROOT = Path(__file__).resolve().parent
APP = "delegate_app/lib"

HEADER_FILL = PatternFill("solid", fgColor="006A64")
HEADER_FONT = Font(bold=True, color="FFFFFF")
THIN = Border(
    left=Side(style="thin"),
    right=Side(style="thin"),
    top=Side(style="thin"),
    bottom=Side(style="thin"),
)


def style_header(ws, row=1):
    for cell in ws[row]:
        cell.fill = HEADER_FILL
        cell.font = HEADER_FONT
        cell.alignment = Alignment(wrap_text=True, vertical="center", horizontal="center")
        cell.border = THIN
    ws.row_dimensions[row].height = 28
    ws.freeze_panes = "A2"
    ws.sheet_view.rightToLeft = True


def autosize(ws, widths):
    for i, w in enumerate(widths, 1):
        ws.column_dimensions[get_column_letter(i)].width = w


# Endpoint | file path | what it calls / does  (exactly 3 columns)
API_ROWS = [
    ("POST https://api.qalataldhaman.com/api/v1/app/Auth/login", f"{APP}/features/auth/data/auth_remote_datasource.dart", "دخول باسم مستخدم وكلمة سر → JWT (access+refresh)"),
    ("POST https://api.qalataldhaman.com/api/v1/app/Auth/logout", f"{APP}/features/auth/data/auth_remote_datasource.dart", "إبطال refresh token عند الخروج"),
    ("GET https://api.qalataldhaman.com/api/v1/app/Users/profile", f"{APP}/features/auth/data/auth_remote_datasource.dart", "اسم المندوب والقائمة بعد الدخول"),
    ("GET https://api.qalataldhaman.com/api/v1/app/Customers", f"{APP}/features/customers/data/customers_remote_datasource.dart", "قائمة زبائن فروع المندوب (JWT) ثم كاش SQLite"),
    ("GET .../Customers/{id}/financial-overview", f"{APP}/features/customers/data/customers_remote_datasource.dart", "إثراء المتبقي/المبيع/القسط اليومي"),
    ("GET .../Customers/{id}/orders", f"{APP}/features/customers/data/customers_remote_datasource.dart", "تحديد activeOrderId للتحصيل"),
    ("POST .../Orders/{orderId}/installment-payments", f"{APP}/features/payments/data/payments_remote_datasource.dart", "رفع تسديد أونلاين"),
    ("POST .../InstallmentPayments/sync", f"{APP}/features/payments/data/payments_remote_datasource.dart", "مزامنة تسديدات أوفلاين"),
    ("GET https://admincompany.alsaaeidy.com/sales-gw/api/mobile-updates/{appKey}", f"{APP}/core/config/app_env.dart", "بوابة التحديثات (مفتاح التطبيق delegate)"),
    ("LOCAL HomeRepositoryImpl.loadHomeSnapshot", f"{APP}/features/home/data/home_repository_impl.dart", "متوسط تقييم المندوب + مجموع القسط + وارد أمس من الكاش"),
    ("LOCAL CustomerRatingCalculator (Dart)", f"{APP}/features/ratings/domain/services/customer_rating_calculator.dart", "متوسط نقاط عينة الزبائن للداشبورد"),
    ("GET {api}/admin/Customers → rating", "api-main/.../GetPaginatedCustomersHandler.cs", "تقييم اسم فقط للوحة المحاسب"),
    ("LOCAL SecureSessionStore", f"{APP}/core/security/secure_session_store.dart", "JWT + userId في Secure Storage"),
]

# layer | path | role | depends_on
ARCH_ROWS = [
    ("إقلاع", f"{APP}/main.dart", "تهيئة Injection وتشغيل App", "injection, app"),
    ("إقلاع", f"{APP}/app.dart", "MaterialApp RTL والثيم", "core/theme"),
    ("إقلاع", f"{APP}/injection.dart", "تسجيل المستودعات وحالات الاستخدام", "كل الطبقات"),
    ("core", f"{APP}/core/theme/app_colors.dart", "لوحة ألوان الهوية", "—"),
    ("core", f"{APP}/core/theme/app_text_styles.dart", "أنماط الخط", "app_colors"),
    ("core", f"{APP}/core/theme/app_theme.dart", "ThemeData موحّد", "app_colors, app_text_styles"),
    ("core", f"{APP}/core/network/dio_client.dart", "Dio + HTTPS إلزامي + قياس زمن الطلب", "secure_session, app_env"),
    ("core", f"{APP}/core/network/api_exception.dart", "أخطاء عربية للواجهة", "—"),
    ("core", f"{APP}/core/security/secure_session_store.dart", "جلسة في flutter_secure_storage", "—"),
    ("core", f"{APP}/core/config/app_env.dart", "عناوين الإنتاج و requireHttps", "—"),
    ("core", f"{APP}/core/database/app_database.dart", "SQLite v2 + عمود date_sale_device", "sqflite"),
    ("core", f"{APP}/core/time/iraq_date.dart", "يوم عمل بغداد", "—"),
    ("core", f"{APP}/core/location/location_service.dart", "GPS حقيقي للتحصيل", "geolocator"),
    ("domain", f"{APP}/features/auth/domain/*", "جلسة المندوب وحالات الدخول/الخروج", "—"),
    ("data", f"{APP}/features/auth/data/*", "API الدخول + تنفيذ المستودع", "domain, dio, secure"),
    ("presentation", f"{APP}/features/auth/presentation/*", "شاشة الدخول فقط", "usecases"),
    ("domain", f"{APP}/features/customers/domain/*", "كيان الزبون + جلب محلي/مزامنة", "—"),
    ("data", f"{APP}/features/customers/data/*", "Remote/Local/Repository للزبائن", "domain, dio, sqlite"),
    ("presentation", f"{APP}/features/customers/presentation/*", "قائمة وتفاصيل الزبون", "usecases"),
    ("domain", f"{APP}/features/payments/domain/*", "قواعد التحصيل والمزامنة", "—"),
    ("data", f"{APP}/features/payments/data/*", "طابور أوفلاين + رفع idempotent", "domain, dio, sqlite"),
    ("presentation", f"{APP}/features/payments/presentation/*", "تحصيل اليوم وورقة المبلغ", "usecases"),
    ("domain", f"{APP}/features/home/domain/*", "DelegateHomeSnapshot + GetHomeSnapshot", "—"),
    ("data", f"{APP}/features/home/data/home_repository_impl.dart", "تقييم المندوب والإعلانات من الكاش المحلي", "customers, payments, session"),
    ("presentation", f"{APP}/features/home/presentation/*", "داشبورد + CurvedNav (زبائن|الرئيسية|تسديدات) + Drawer", "home usecase"),
    ("domain", f"{APP}/features/ratings/domain/services/customer_rating_calculator.dart", "حاسبة نقاط الزبون للمندوب", "—"),
    ("presentation", f"{APP}/features/ratings/presentation/ratings_overview_page.dart", "ملخص مستويات التقييم من المنيو", "calculator + customers"),
    ("application", "api-main/.../Customers/Common/CustomerRatingCalculator.cs", "حاسبة اسم التقييم للمحاسب الرئيسي", "—"),
    ("application", "api-main/.../GetPaginatedCustomersHandler.cs", "يرجع rating في قائمة العملاء", "CustomerRatingCalculator"),
    ("presentation", "panel-admin-main/.../CustomerList.vue", "عمود التقييم (اسم فقط)", "GET /admin/Customers"),
    ("presentation", f"{APP}/features/sync/presentation/sync_status_banner.dart", "حالة الطابور", "payments"),
    ("presentation", f"{APP}/features/receipts/*", "PDF الإيصال", "pdf"),
]

TREE_LINES = [
    "delegate_app/",
    "  lib/",
    "    1_bootstrap → main.dart | app.dart | injection.dart",
    "    2_core/",
    "      config/ | database/ | location/ | network/ | security/ | theme/ | time/ | widgets/",
    "    3_features/",
    "      auth/ → domain | data | presentation",
    "      customers/ → domain | data | presentation",
    "      payments/ → domain | data | presentation",
    "      home/ → domain | data | presentation (dashboard + CurvedNav 3 tabs)",
    "      ratings/ → domain/services + presentation/overview",
    "      sync/ → presentation",
    "      receipts/ → data | presentation",
    "panel-admin-main/ → CustomerList عمود التقييم (اسم فقط)",
    "api-main/ → CustomerRatingCalculator + GetPaginatedCustomers.rating",
    "    القاعدة: presentation → usecase → repository(interface) → data impl",
]


def write_api_excel():
    wb = Workbook()
    ws = wb.active
    ws.title = "API_Registry"
    ws.append(["Endpoint", "مسار الملف المستخدم به", "ماذا يفعل / ماذا يستدعي"])
    for row in API_ROWS:
        ws.append(list(row))
    style_header(ws)
    autosize(ws, [55, 70, 70])
    out = ROOT / "Delegate_API_Documentation.xlsx"
    wb.save(out)
    return out


def write_architecture_excel():
    wb = Workbook()
    ws = wb.active
    ws.title = "Architecture_Map"
    ws.append(["الطبقة", "المسار", "الدور", "يعتمد على"])
    for row in ARCH_ROWS:
        ws.append(list(row))
    style_header(ws)
    autosize(ws, [14, 75, 55, 35])

    ws2 = wb.create_sheet("Tree")
    ws2.append(["المستوى / الفرع"])
    for line in TREE_LINES:
        ws2.append([line])
    style_header(ws2)
    autosize(ws2, [90])

    ws3 = wb.create_sheet("Ratings_Feature")
    ws3.append(["الترتيب", "المسار", "الطبقة", "ملاحظة"])
    rating_detail = [
        (1, "api-main/.../Customers/Common/CustomerRatingCalculator.cs", "application", "قواعد العمل — اسم فقط (محاسب)"),
        (2, "api-main/.../GetPaginatedCustomersHandler.cs", "application", "يضيف rating للقائمة"),
        (3, "api-main/.../GetCustomersExcelReportHandler.cs", "application", "عمود التقييم في Excel"),
        (4, "panel-admin-main/.../CustomerList.vue", "presentation", "عمود التقييم في الجدول"),
        (5, "panel-admin-main/.../Customer/localization/ar.json", "i18n", "عنوان العمود: التقييم"),
        (6, f"{APP}/features/ratings/domain/services/customer_rating_calculator.dart", "domain", "متوسط نقاط المندوب من كاش الزبائن"),
        (7, f"{APP}/features/home/data/home_repository_impl.dart", "data", "يحسب currentRating للداشبورد"),
        (8, f"{APP}/features/ratings/presentation/ratings_overview_page.dart", "presentation", "ملخص المستويات من Drawer"),
    ]
    for r in rating_detail:
        ws3.append(list(r))
    style_header(ws3)
    autosize(ws3, [10, 75, 14, 25])

    out = ROOT / "Clean_Architecture_Map.xlsx"
    wb.save(out)
    return out


if __name__ == "__main__":
    a = write_api_excel()
    b = write_architecture_excel()
    print(f"API Excel: {a}")
    print(f"Architecture Excel: {b}")
