# -*- coding: utf-8 -*-
"""مولّد قابل لإعادة التشغيل لتوثيق مرحلة تأسيس اللوحة الموحدة.
لا يتضمن أسرارًا أو بيانات شخصية.
التشغيل: python generate_docs.py
المتطلبات: openpyxl, python-docx
"""
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from openpyxl.utils import get_column_letter
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Pt

ROOT = Path(__file__).resolve().parent

HEADER_FILL = PatternFill("solid", fgColor="006A64")
HEADER_FONT = Font(color="FFFFFF", bold=True, name="Arial")
BODY_FONT = Font(name="Arial", size=11)
THIN = Border(
    left=Side(style="thin"),
    right=Side(style="thin"),
    top=Side(style="thin"),
    bottom=Side(style="thin"),
)

MODULES = [
    ("home", "الرئيسية", "HomeMainRoute", "/", "overview", "publicAuthenticated", "ready", "أي مستخدم مسجّل"),
    ("branch", "الفروع", "BranchListRoute", "/branch", "operations", "single: Branch.Read", "ready", "من مسار Branch"),
    ("payment", "التسديدات", "PaymentListRoute", "/payment", "operations", "single: OrderList.Read", "ready", "مؤكد — ليس InstallmentPayment.Read"),
    ("customer", "العملاء", "CustomerMainRoute", "/customer", "operations", "single: Customer.Read", "ready", ""),
    ("purchases", "الحسابات", "PurchasesListRoute", "/purchases", "finance", "single: Purchase.Read", "ready", ""),
    ("expenses", "المصاريف", "ExpensesListRoute", "/expenses", "finance", "single: Expense.Read", "ready", ""),
    ("sale", "المبيعات", "SaleListRoute", "/sale", "operations", "single: Order.Read", "ready", ""),
    ("warehouse", "المخزن", "WarehouseListRoute", "/warehouse", "inventory", "single: ProductCategory.Read", "ready", "كما في مسار الوحدة الحالي"),
    ("safeMain", "القاصة الرئيسية", "SafeMainMainRoute", "/safe-main", "finance", "single: Safe.Read", "ready", ""),
    ("safe", "القاصه", "SafeMainRoute", "/safe", "finance", "single: Safe.Read", "ready", ""),
    ("order", "القوائم", "OrderListRoute", "/order", "operations", "single: OrderList.Read", "ready", ""),
    ("user", "المستخدمين", "UserListRoute", "/user", "people", "single: User.Read", "ready", ""),
    ("activityLog", "سجل المستخدمين", "ActivityLogListRoute", "/activity-log", "system", "single: ActivityLog.Read", "ready", ""),
    ("notifications", "الأشعارات", "NotificationListRoute", "/notification", "system", "single: Notification.Read", "ready", "بادج عدد غير المقروء عند السماح فقط"),
]

PERMISSIONS = [
    ("Branch.Read", "قراءة الفروع", "Branch", "Read", "قائمة الفروع والتنقل"),
    ("OrderList.Read", "قراءة قوائم الطلبات", "OrderList", "Read", "التسديدات + القوائم"),
    ("Customer.Read", "قراءة العملاء", "Customer", "Read", "قسم العملاء"),
    ("Purchase.Read", "قراءة المشتريات", "Purchase", "Read", "الحسابات"),
    ("Expense.Read", "قراءة المصاريف", "Expense", "Read", "المصاريف"),
    ("Order.Read", "قراءة الطلبات/المبيعات", "Order", "Read", "المبيعات"),
    ("ProductCategory.Read", "قراءة أصناف المنتجات", "ProductCategory", "Read", "المخزن (الوضع الحالي)"),
    ("Safe.Read", "قراءة القاصات", "Safe", "Read", "القاصة والقاصة الرئيسية"),
    ("User.Read", "قراءة المستخدمين", "User", "Read", "قسم المستخدمين + بطاقة الملف في الرئيسية"),
    ("User.Create", "إنشاء مستخدم", "User", "Create", "إنشاء حساب — لا يُستنتج من Read"),
    ("User.Update", "تعديل مستخدم", "User", "Update", "يشمل حاليًا PUT permissions (مؤجل إعادة تصميم)"),
    ("ActivityLog.Read", "قراءة سجل النشاط", "ActivityLog", "Read", "سجل المستخدمين"),
    ("Notification.Read", "قراءة الإشعارات", "Notification", "Read", "الإشعارات وعداد غير المقروء"),
    ("Report.Read", "قراءة التقارير", "Report", "Read", "تقارير لوحة الرئيسية المالية/المخزنية"),
    ("Permission.Read", "قراءة كتالوج الصلاحيات", "Permission", "Read", "إدارة فقط — ليس شرط دخول اللوحة"),
    ("Role.Read", "قراءة الأدوار", "Role", "Read", "إدارة فقط"),
]

ENDPOINTS = [
    ("GET", "/api/v1/admin/Reports/yearly-financial", "Report.Read", "فروع المستخدم أو Admin", "branchId, year?", "GetYearlyFinancialReportResponse", "ReportsController + GetYearlyFinancialReportHandler", "403 صلاحية؛ رفض نطاق فرع"),
    ("GET", "/api/v1/admin/Reports/users", "Report.Read", "فروع المستخدم أو Admin", "branchId", "GetUsersReportResponse", "ReportsController + GetUsersReportHandler", "403؛ رفض نطاق فرع"),
    ("GET", "/api/v1/admin/Reports/safe", "Report.Read", "فروع المستخدم أو Admin", "branchId", "GetSafeReportResponse", "ReportsController + GetSafeReportHandler", "403؛ رفض نطاق فرع"),
    ("GET", "/api/v1/admin/Reports/products", "Report.Read", "فروع المستخدم أو Admin", "branchId", "GetProductsReportResponse", "ReportsController + GetProductsReportHandler", "403؛ رفض نطاق فرع"),
    ("GET", "/api/v1/admin/Users/profile", "User.Read", "حسب تنفيذ Users", "branchId", "UserProfileResponse", "UsersController profile", "403 دون User.Read"),
    ("GET", "/api/v1/admin/Permissions", "Permission.Read", "إدارة", "—", "قائمة صلاحيات", "Permissions admin", "لا يُطلب لدخول اللوحة"),
    ("GET", "/api/v1/admin/Roles", "Role.Read", "إدارة", "—", "قائمة أدوار", "Roles admin", "لا يُطلب لدخول اللوحة"),
    ("GET", "/api/v1/admin/Roles/{id}/permissions", "Permission.Read", "إدارة", "id", "صلاحيات الدور", "Roles admin", ""),
    ("GET", "/api/v1/admin/Users/{id}", "User.Read", "إدارة مستخدمين", "id", "User DTO", "UsersController", ""),
    ("POST", "/api/v1/admin/Users", "User.Create", "إدارة مستخدمين", "CreateUser DTO", "User", "CreateUserHandler", ""),
    ("PUT", "/api/v1/admin/Users/{id}/permissions", "User.Update", "إدارة مستخدمين", "permissions payload", "نجاح + إبطال جلسات", "UpdateUserPermissionsHandler", "عقد السماح/المنع مؤجل"),
    ("POST", "/api/v1/admin/Auth/login", "عام", "—", "SignInRequest", "tokens + permissions + branches", "AuthService", "مصدر صلاحيات الجلسة للوحة"),
    ("POST", "/api/v1/admin/Auth/refresh-token", "refresh", "—", "refreshToken", "SignInResponse (tokens+permissions+branches)", "refreshHttp + sessionRefreshCoordinator", "منسّق واحد لكل جيل؛ يحدّث الواجهة عبر applyAuthPayload"),
]

UI_API_MAP = [
    ("TheSideBar / عنصر مقفل", "لا طلب", "state=locked → AccessDeniedRoute"),
    ("TheSideBar / إشعارات badge", "GET Notifications unread-count", "Notification.Read + branchId"),
    ("DashboardContent / بطاقة مستخدم", "GET Users/profile", "User.Read + branchId في queryKey"),
    ("DashboardContent / مخطط شهري", "GET Reports/yearly-financial", "Report.Read + branchId"),
    ("DashboardContent / إحصائيات", "GET Reports/users", "Report.Read + branchId"),
    ("DashboardContent / جدول مالي", "GET Reports/safe", "Report.Read + branchId"),
    ("DashboardContent / مخزن", "GET Reports/products", "Report.Read + branchId"),
    ("PermissionGate", "—", "evaluateAccess على requirement معلن"),
    ("authGuard مسار محمي", "—", "meta.permission أو registry؛ AccessDenied بدون loop"),
    ("logout / replaceSession", "إلغاء ومسح QueryClient", "clearSessionQueryCache"),
    ("changeSelectedBranch", "مسح الكاش ثم تبديل الفرع", "مفاتيح الاستعلام تتضمن branchId"),
    ("axios interceptor + sessionGeneration", "POST Auth/refresh-token (منسّق)", "جيل ملتقط عند الإنشاء؛ لا إعادة بتوكن حساب آخر"),
    ("bootstrapSession", "نفس منسّق refresh", "لا تدوير متزامن مع 401 لنفس الجيل"),
    ("applyAuthPayload(source=refresh)", "invalidateSessionQueries عند تغيّر نطاق", "لا cancelBranchQueries على تجديد توكن فقط"),
    ("NotFoundRoute / SessionBootRoute", "—", "استثناءات مسارات خدمية معلنة صراحة"),
]

ACCEPTANCE = [
    ("AC-01", "صلاحية موجودة/مفقودة في evaluateAccess", "allowed / denied", "passed", "vitest evaluateAccess"),
    ("AC-02", "allOf / anyOf", "سلوك منطقي صحيح", "passed", "vitest"),
    ("AC-03", "شرط غير صالح أو مفقود", "denied", "passed", "vitest"),
    ("AC-04", "حالة loading لا تتحول إلى سماح", "canAccess=false", "passed", "vitest"),
    ("AC-05", "مستخدم Read يفتح القسم", "رابط فعّال بدون قفل", "manual/ui", "لقطة mobile-allowed-payment"),
    ("AC-06", "بدون صلاحية يرى القفل", "زر مقفل + AccessDenied", "manual/ui", "لقطة mobile-locked-denied"),
    ("AC-07", "رابط مباشر لا يتجاوز", "AccessDeniedRoute / boot", "passed", "resolveRouteAccess vitest"),
    ("AC-08", "قسم مقفل لا يرسل بيانات", "enabled=false في query", "passed-code", "Report queries gated"),
    ("AC-09", "إخفاء Create لا يمنع Read", "قائمة تظهر بـ Read", "documented", "لا ربط Create↔Read"),
    ("AC-10", "تبديل حساب يمسح الكاش", "endSession + generation", "passed", "lateResponseIsolation"),
    ("AC-11", "تبديل فرع بجيل منفصل", "branchGeneration دون sessionGeneration", "passed", "lateResponseIsolation"),
    ("AC-12", "Drawer + Escape + لوحة مفاتيح", "focus trap + إغلاق", "manual/ui", "لقطة drawer-open + كود"),
    ("AC-13", "لا redirect loop عند المنع", "AccessDeniedRoute", "passed-code", "guard"),
    ("AC-14", "تقرير خارج نطاق الفرع", "رفض Report_Branch_Access_Denied", "passed", "xunit ReportBranchAccessTests"),
    ("AC-15", "01.1 تحميل صلاحيات → boot لا يركّب الصفحة", "outcome=boot remember", "passed", "resolveRouteAccess"),
    ("AC-16", "01.1 مسار محمي بلا شرط + فرعي", "denied / شرط من matched", "passed", "resolveRouteAccess"),
    ("AC-17", "01.1 استجابة متأخرة A بعد B", "لا تُطبَّق", "passed", "lateResponseIsolation"),
    ("AC-18", "01.1 تغيّر صلاحيات يعيد التقييم", "denied بعد الفقدان", "passed", "permissionChangeRevalidation"),
    # —— 01.2: اختبارات دوال (وحدة) ——
    ("AC-19", "01.2 مقارنة أجيال الجلسة", "isStaleGeneration", "passed-unit", "sessionGeneration.test"),
    ("AC-20", "01.2 مسار غير معلن بلا meta/registry", "denied — لا allow لمجرّد التسجيل", "passed-unit", "resolveRouteAccess.test"),
    ("AC-21", "01.2 مسارات عامة معلنة صراحةً", "publicAuthenticated / EXPLICIT_PUBLIC", "passed-unit", "resolveRouteAccess.test"),
    # —— 01.2: تكامل Store + HTTP (Pinia + Axios adapter) ——
    ("AC-22", "01.2 أ) 200 لحساب A بعد دخول B", "لا تُمرَّر لجلسة B؛ إلغاء صامت", "passed-integration", "sessionHttp.integration"),
    ("AC-23", "01.2 ب) 401 لحساب A بعد دخول B", "لا refresh/إعادة بتوكن B", "passed-integration", "sessionHttp.integration"),
    ("AC-24", "01.2 ج) فشل refresh لجيل A بعد B", "جلسة B وطابورها سليمين", "passed-integration", "sessionHttp.integration"),
    ("AC-25", "01.2 د) طلبان 401 متزامنان", "refresh واحد + إعادة محدودة + إكمال الاثنين", "passed-integration", "sessionHttp.integration"),
    ("AC-26", "01.2 هـ) bootstrap و401 متزامنان", "لا تدوير متزامن لنفس refresh token", "passed-integration", "sessionHttp.integration"),
    ("AC-27", "01.2 و) انتهاء الجلسة أثناء الطابور", "رفض كل الوعود بلا تعليق", "passed-integration", "sessionHttp.integration"),
    ("AC-28", "01.2 ز) refresh عادي ناجح", "لا cancelBranchQueries على الطلب المنتظر", "passed-integration", "sessionHttp.integration"),
    # —— 01.2: تكامل Router ——
    ("AC-29", "01.2 ح) مسار غير معلن", "AccessDenied؛ لا beforeEnter لصفحة الأعمال", "passed-integration", "routerGuard.integration"),
    ("AC-30", "01.2 ط) أثناء loading", "SessionBoot؛ لا beforeEnter للصفحة المحمية", "passed-integration", "routerGuard.integration"),
    # —— إصلاح لاحق: retry بعد فشل مؤقت ——
    ("AC-34", "فشل شبكة bootstrap ثم retry", "طلب HTTP ثانٍ + permissions ready", "passed-integration", "bootstrapRetry.integration"),
    ("AC-35", "retry متزامن بعد فشل مؤقت", "refresh واحد مشترك", "passed-integration", "bootstrapRetry.integration"),
    ("AC-36", "رفض سيرفر لـ refresh token", "endSession؛ لا إعادة لا نهائية", "passed-integration", "bootstrapRetry.integration"),
    # —— ما لم يُختبر ——
    ("AC-31", "E2E متصفح حقيقي لسباق الجلسة", "—", "untested", "لا اتصال إنتاج؛ adapter محلي فقط"),
    ("AC-32", "تجديد توكن منتهي بعد ساعات في جلسة واحدة", "عملية refresh ثانية بعد settled", "untested-manual", "منسّق يفتح دورة جديدة عند تغيّر refreshToken"),
    ("AC-33", "تغيير صلاحيات/فروع عبر refresh الحي", "invalidate دون cancel للطلب المنتظر", "passed-code", "applyAuthPayload source=refresh + scopeChanged"),
]

FOLLOWUPS = [
    ("Allow/Deny صريح لصلاحيات المستخدم", "العقد الحالي UserPermissions إضافة فقط؛ الواجهة تعرض readonly للدور", "مرحلة 02"),
    ("عدم منح Permissions.All ضمنيًا لبعض المحاسبين", "Roles seed موجودة — لا توسيع في هذه المرحلة", "مرحلة 02"),
    ("فصل User.Update عن إدارة صلاحيات النظام", "PUT permissions يستخدم User.Update", "مرحلة 02"),
    ("مراجعة OrderList.Read مقابل InstallmentPayment للتسديدات", "موثّق كقرار حالي", "مراجعة منتج"),
    ("نقل وظائف اللوحات القديمة", "خارج نطاق التأسيس", "مراحل لاحقة"),
    ("endpoint /me أو /capabilities", "غير موجود؛ التحقق عبر refresh الفعلي", "فقط عند حاجة مثبتة"),
    ("اختبارات E2E للمتصفح لـ Drawer/focus", "لقطات يدوية/Playwright محلية؛ لا CI E2E بعد", "تحسين تحقق"),
    ("إلغاء mutations وصلت للسيرفر", "مسح الكاش لا يلغي الكتابة الناجحة على السيرفر", "قيود معمارية موثّقة"),
    ("إعادة تصميم المصادقة/الأدوار", "خارج 01.2 — لا توسعة", "مرحلة لاحقة"),
]


def style_header(ws, headers):
    ws.append(headers)
    for col, _ in enumerate(headers, 1):
        cell = ws.cell(1, col)
        cell.fill = HEADER_FILL
        cell.font = HEADER_FONT
        cell.alignment = Alignment(horizontal="center", wrap_text=True, vertical="center")


def style_body(ws, ncols):
    for row in ws.iter_rows(min_row=1, max_row=ws.max_row, max_col=ncols):
        for cell in row:
            cell.border = THIN
            if cell.row > 1:
                cell.font = BODY_FONT
            cell.alignment = Alignment(wrap_text=True, vertical="top")
    for col in range(1, ncols + 1):
        values = [str(c.value or "") for c in ws[get_column_letter(col)]]
        width = min(max(len(v) for v in values) + 2, 48)
        ws.column_dimensions[get_column_letter(col)].width = width
    ws.sheet_view.rightToLeft = True


def write_excel():
    wb = Workbook()

    ws = wb.active
    ws.title = "Modules"
    headers = ["id", "القسم", "routeName", "المسار", "المجموعة", "شرط الوصول", "حالة التنفيذ", "ملاحظة"]
    style_header(ws, headers)
    for row in MODULES:
        ws.append(list(row))
    style_body(ws, len(headers))

    ws = wb.create_sheet("Permissions")
    headers = ["المفتاح", "الوصف", "المورد", "العملية", "الاستخدام الحالي"]
    style_header(ws, headers)
    for row in PERMISSIONS:
        ws.append(list(row))
    style_body(ws, len(headers))

    ws = wb.create_sheet("Endpoints")
    headers = ["Method", "Path", "الصلاحية", "نطاق البيانات", "Request", "Response DTO", "ملفات التنفيذ", "حالات خطأ مؤكدة"]
    style_header(ws, headers)
    for row in ENDPOINTS:
        ws.append(list(row))
    style_body(ws, len(headers))

    ws = wb.create_sheet("UI_API_Map")
    headers = ["المكون أو الأكشن", "endpoint المرتبط", "شرط تشغيل الطلب"]
    style_header(ws, headers)
    for row in UI_API_MAP:
        ws.append(list(row))
    style_body(ws, len(headers))

    ws = wb.create_sheet("Acceptance")
    headers = ["المعرّف", "السيناريو", "النتيجة المتوقعة", "حالة الاختبار", "الدليل"]
    style_header(ws, headers)
    for row in ACCEPTANCE:
        ws.append(list(row))
    style_body(ws, len(headers))

    ws = wb.create_sheet("Followups")
    headers = ["العمل المؤجل", "السبب / الواقع", "المرحلة"]
    style_header(ws, headers)
    for row in FOLLOWUPS:
        ws.append(list(row))
    style_body(ws, len(headers))

    out = ROOT / "Unified_Admin_Foundation.xlsx"
    wb.save(out)
    return out


def add_heading_ar(doc, text, level=1):
    p = doc.add_heading(text, level=level)
    p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    return p


def add_para(doc, text):
    p = doc.add_paragraph(text)
    p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    for run in p.runs:
        run.font.size = Pt(12)
        run.font.name = "Arial"
    return p


def write_word():
    doc = Document()
    title = doc.add_heading("تأسيس اللوحة الموحدة — قلعة الضمان", 0)
    title.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    add_para(doc, "وثيقة واقع منفَّذ لمرحلة التأسيس. تميّز الموجود عن المقترح والمؤجل.")

    add_heading_ar(doc, "1) مسؤوليات الطبقات", 1)
    add_para(doc, "navigation.registry: بيانات الأقسام المنفَّذة وربطها بشروط الوصول.")
    add_para(doc, "evaluateAccess: دوال نقية بلا HTTP/UI.")
    add_para(doc, "usePermission: ربط Pinia التفاعلي.")
    add_para(doc, "useSideBar: اشتقاق عناصر القائمة وحالات allowed|locked.")
    add_para(doc, "PermissionGate / AccessDeniedState: عرض مشروط دون تسريب بيانات.")
    add_para(doc, "authGuard: نفس قواعد السجل/meta؛ AccessDenied بدون حلقة إعادة توجيه.")
    add_para(doc, "تقارير API: Report.Read + تحقق نطاق الفرع في طبقة التطبيق.")

    add_heading_ar(doc, "2) شجرة ملفات المرحلة", 1)
    files = [
        ("جديد", "panel-admin-main/.../Core/access/access.types.ts", "أنواع شرط الوصول"),
        ("جديد", "panel-admin-main/.../Core/access/evaluateAccess.ts", "تقييم نقي"),
        ("جديد", "panel-admin-main/.../Core/navigation/navigation.types.ts", "أنواع الأقسام"),
        ("جديد", "panel-admin-main/.../Core/navigation/navigation.registry.ts", "سجل الأقسام الجاهزة"),
        ("جديد", "panel-admin-main/.../Core/components/access/PermissionGate.vue", "بوابة عرض"),
        ("جديد", "panel-admin-main/.../Core/components/access/AccessDeniedState.vue", "حالة المنع"),
        ("جديد", "panel-admin-main/.../Core/views/AccessDeniedView.vue", "صفحة المنع"),
        ("معدل", "usePermission.ts / useSideBar.ts / guard.ts / TheSideBar.vue / TheDefaultLayout.vue", "ربط السلوك"),
        ("معدل", "vue-query.ts / Auth store / LoginPage / Dashboard queries", "كاش الجلسة وبوابات الاستعلام"),
        ("جديد", "api-main/.../Reports/Common/ReportBranchAccess.cs", "نطاق الفرع"),
        ("معدل", "Handlers تقارير الأربعة", "رفض خارج النطاق"),
        ("جديد", "docs/unified-admin-foundation/*", "Excel/Word/مولّد"),
    ]
    for status, path, role in files:
        add_para(doc, f"[{status}] {path} — {role}")

    add_heading_ar(doc, "3) رحلة الدخول ثم التنقل", 1)
    add_para(doc, "login → replaceSession (مسح كاش سابق) → حفظ tokens والصلاحيات والفروع → تحويل الصلاحيات في Pinia → useSideBar يقيّم كل قسم جاهز → عنصر مسموح كرابط / مقفل كزر لوحة مفاتيح.")

    add_heading_ar(doc, "4) فتح قسم مسموح ومقفول", 1)
    add_para(doc, "مسموح: تنقل Router + تحميل الصفحة وطلبات البيانات حسب صلاحيات الصفحة.")
    add_para(doc, "مقفل من القائمة: لا تحميل مكوّن الصفحة؛ انتقال إلى AccessDenied برسالة قصيرة دون زر طلب صلاحية.")
    add_para(doc, "رابط مباشر: authGuard يطبّق نفس الشرط ويستبدل المسار بـ AccessDeniedRoute.")

    add_heading_ar(doc, "5) إدارة Query cache", 1)
    add_para(doc, "عند الخروج أو استبدال الجلسة: endSession → abort + cancelQueries + clear + زيادة sessionGeneration.")
    add_para(doc, "عند تبديل الفرع: زيادة branchGeneration + cancelQueries دون خلطه بهوية الحساب.")
    add_para(doc, "تقارير لوحة الرئيسية لا تُفعَّل دون Report.Read (وUser.Read للملف)؛ queryFn يمرّر AbortSignal.")

    add_heading_ar(doc, "6) إضافة تبويب جديد مستقبلًا", 1)
    add_para(doc, "1) أضف مسار الوحدة مع meta.permission الحقيقي.")
    add_para(doc, "2) سجّل القسم في NAVIGATION_REGISTRY بحالة ready فقط إن كان منفَّذًا.")
    add_para(doc, "3) لا تعتمد على اسم الدور؛ استخدم مفتاح الصلاحية.")
    add_para(doc, "4) اربط طلبات البيانات بـ enabled حسب الصلاحية وbranchId.")
    add_para(doc, "5) حدّث Excel عبر تشغيل هذا المولّد.")

    add_heading_ar(doc, "7) تصحيح 01.1 — ما نُفّذ", 1)
    add_para(doc, "دورة صلاحيات loading/ready/error؛ النسخة المحلية ليست إثبات جاهزية (reclaim + refresh فعلي).")
    add_para(doc, "SessionBootRoute: لا تركيب صفحة أعمال أثناء التحميل؛ حفظ الوجهة وإعادة التقييم بعد الجاهزية.")
    add_para(doc, "resolveRouteAccess: حماية بالسجل/meta حتى مع نسيان requiresAuth؛ مسارات فرعية عبر matched.")
    add_para(doc, "endSession مركزي؛ عزل أجيال الجلسة/الفرع؛ رفض refresh قديم بعد استبدال الحساب؛ تحديث permissions/branches من عقد SignInResponse عند refresh.")
    add_para(doc, "Drawer: إغلاق بعد التنقل/القسم الحالي، زر إغلاق، إخفاء rail على الهاتف، Escape/focus/inert، useMediaQuery.")

    add_heading_ar(doc, "8) نتائج التحقق 01.1", 1)
    add_para(doc, "vitest: 21 اختبارًا ناجحًا (evaluateAccess + resolveRouteAccess + sessionGeneration + lateResponse + permissionChange).")
    add_para(doc, "vite build-only: ناجح.")
    add_para(doc, "vue-tsc: 8 أخطاء في ملفات غير معدّلة بهذا التصحيح (CustomerSelectInput, ProductSelectInput, Notification/enums, OrderForm, SafeByIdQuery, CreateSaleMutation) — الدليل: git status نظيف لتلك المسارات؛ آخر لمس تاريخي main project file.")
    add_para(doc, "لقطات هاتف: mobile-360-01.1-drawer-open / allowed-payment / locked-denied.")

    add_heading_ar(doc, "9) تصحيح 01.2 — دورة الجلسة والمسارات", 1)
    add_para(doc, "عزل Axios: التقاط sessionGeneration عند إنشاء الطلب؛ فحص الجيل قبل قبول 200/عرض خطأ/401/refresh/إعادة الإرسال/الطابور؛ إلغاء صامت stale_session؛ دمج إشارة الجلسة مع إشارة الطلب (AbortSignal.any أو مسار احتياطي مع تنظيف listeners).")
    add_para(doc, "تنسيق refresh: sessionRefreshCoordinator يربط inflight بجيل الجلسة قبل أي await؛ bootstrap وinterceptor يشتركان؛ finally لا يصفّر مالك جيل أحدث؛ فشل جيل قديم لا يرفض طابور الأحدث؛ abortRefresh عند endSession.")
    add_para(doc, "إصلاح لاحق: لا يُعاد استخدام Promise مرفوضة بعد فشل مؤقت؛ التنظيف بهوية العملية؛ رفض 401/403 للتوكن ينهي الجلسة ويمنع إعادة لا نهائية؛ النجاح بنفس التوكن يبقى قابلًا لمشاركة المنضم المتأخر.")
    add_para(doc, "applyAuthPayload: source=refresh لا يستدعي cancelBranchQueries؛ عند تغيّر صلاحيات/فروع فقط invalidateSessionQueries. لا خلط بين تجديد التوكن وتبديل الفرع/الحساب.")
    add_para(doc, "resolveRouteAccess: المسارات العامة معلنة صراحة؛ غير المعلن بلا شرط معروف → denied حتى مع مستخدم مسجّل. NotFound وSessionBoot استثناءات خدمية معلنة.")

    add_heading_ar(doc, "10) نتائج التحقق 01.2 وتصنيف الاختبارات", 1)
    add_para(doc, "اختبارات دوال (وحدة): evaluateAccess، resolveRouteAccess، sessionGeneration، lateResponseIsolation، permissionChangeRevalidation.")
    add_para(doc, "اختبارات تكامل Store+HTTP: sessionHttp.integration — سيناريوهات أ–ز بـ Pinia + Axios adapter محلي (ترتيب استجابات متحكّم؛ بلا إنتاج).")
    add_para(doc, "اختبارات تكامل Router: routerGuard.integration — loading بلا beforeEnter للصفحة المحمية؛ مسار غير معلن → AccessDenied؛ مسار مسموح يستدعي beforeEnter.")
    add_para(doc, "اختبارات تكامل bootstrapRetry: فشل شبكة ثم retry بطلب ثانٍ ناجح؛ retry متزامن يشارك refresh واحد؛ رفض توكن من السيرفر → endSession دون إعادة لا نهائية.")
    add_para(doc, "ما لم يُختبر: E2E متصفح حقيقي لسباقات الشبكة الطويلة؛ دورة refresh ثانية بعد ساعات في نفس الجلسة (منسّق يدعمها عند تغيّر refreshToken لكن بلا اختبار زمني).")
    add_para(doc, "vitest: 36 اختبارًا ناجحًا (بعد إصلاح Promise المرفوضة). vite build-only: ناجح. npm run build يعتمد yarn غير المتوفر محليًا — استخدم build-only.")
    add_para(doc, "vue-tsc: 8 أخطاء متبقية في ملفات خارج نطاق 01.2 (نفس مجموعة 01.1؛ axios GenericAbortSignal أُصلح في 01.2). لا baseline آلي محفوظ في المستودع؛ قدم الأخطاء غير مثبت بـ git blame لكل ملف، وعلاقتها بتغييرات 01.2: لا تداخل مسارات مشتركة بعد إصلاح axios.")

    add_heading_ar(doc, "11) حدود المرحلة والقرارات المؤجلة", 1)
    add_para(doc, "لا نقل وظائف اللوحات القديمة. لا تغيير عقد Allow/Deny (مرحلة 02). لا منح صلاحيات جديدة للحسابات. لا Flutter. لا إنتاج. لا إعادة تصميم المصادقة/الأدوار في 01.2.")
    add_para(doc, "صلاحيات الدور تبقى موروثة؛ الواجهة لا توهم بإطفاء موروث غير مدعوم من السيرفر.")
    add_para(doc, "مسح/إلغاء الطلبات لا يلغي mutation وصلت للسيرفر بنجاح قبل الإلغاء.")

    out = ROOT / "Unified_Admin_Foundation_Architecture.docx"
    doc.save(out)
    return out


def main():
    xlsx = write_excel()
    docx = write_word()
    print(f"Wrote {xlsx}")
    print(f"Wrote {docx}")


if __name__ == "__main__":
    main()
