# -*- coding: utf-8 -*-
"""Generate Word architecture + Excel API endpoints for committee_officer_application.

نظام قلعة الضمان (api.qalataldhaman.com) — نفس أكشنات تطبيق المتابع، APIs مختلفة عن admincompany.
"""
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Pt
from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side

ROOT = Path(__file__).resolve().parent

# Method, Path, Auth, Request, Response, Purpose, Area, ClientFile, Permission, Status
ENDPOINTS = [
    ("POST", "api/v1/app/Auth/login", "لا", "{userName,password}", "{accessToken,refreshToken,userId,fullName}", "دخول JWT", "Auth", "features/auth/data/auth_remote.dart", "Public", "جاهز"),
    ("POST", "api/v1/app/Auth/refresh-token", "Refresh", "{refreshToken}", "{accessToken,refreshToken}", "تجديد", "Auth", "features/auth/data/auth_remote.dart", "Public", "جاهز"),
    ("POST", "api/v1/app/Auth/logout", "Bearer", "{refreshToken}", "204", "خروج", "Auth", "features/auth/data/auth_remote.dart", "Auth", "جاهز"),
    ("GET", "api/v1/app/committee/me", "Bearer", "—", "{userId,fullName,role,isCommitteeOfficer,...}", "بوابة دور مسؤول اللجنة", "Committee", "features/auth/data/auth_remote.dart", "OrderList.Read", "جاهز"),
    ("GET", "api/v1/app/committee/lists", "Bearer", "—", "CommitteeListDto[]", "قوائم المسؤول", "Committee", "features/home/data/committee_remote.dart", "OrderList.Read", "جاهز"),
    ("GET", "api/v1/app/committee/list-cards", "Bearer", "—", "CommitteeListCardDto[]", "بطاقات الرئيسية", "Committee", "features/home/data/committee_remote.dart", "OrderList.Read", "جاهز"),
    ("GET", "api/v1/app/committee/daily", "Bearer", "listId,date?,showType", "CommitteeDailyResponse", "يومي مسدد/غير مسدد", "Committee", "features/home/data/committee_remote.dart", "OrderList.Read", "جاهز"),
    ("GET", "api/v1/app/committee/collection-lists", "Bearer", "—", "CommitteeCollectionListDto[]", "قوائم الجباية", "Committee", "features/home/data/committee_remote.dart", "OrderList.Read", "جاهز"),
    ("GET", "api/v1/app/committee/declining-customers", "Bearer", "—", "CommitteeDecliningCustomerDto[]", "زبائن متدنية", "Committee", "features/home/data/committee_remote.dart", "Customer.Read", "جاهز"),
    ("GET", "api/v1/app/committee/pending-sales", "Bearer", "—", "CommitteePendingSaleDto[]", "مبيعات معلّقة للتصديق", "Committee", "features/home/data/committee_remote.dart", "Order.Read", "جاهز"),
    ("POST", "api/v1/app/committee/pending-sales/{orderId}/authenticate", "Bearer", "—", "204", "تصديق مبيع", "Committee", "features/home/data/committee_remote.dart", "Order.Read", "جاهز"),
    ("GET", "api/v1/app/committee/day-ratings/missing", "Bearer", "date?", "CommitteeMissingDayRatingDto[]", "تقييمات مندوب ناقصة", "Committee", "features/home/data/committee_remote.dart", "OrderList.Read", "جاهز"),
    ("POST", "api/v1/app/committee/day-ratings", "Bearer", "{listId,date?,score,note?}", "204", "إرسال تقييم يومي 0-10", "Committee", "features/home/data/committee_remote.dart", "OrderList.Read", "جاهز"),
    ("POST", "api/v1/app/committee/complaints", "Bearer", "{body,subject?}", "204", "إرسال شكوى", "Committee", "features/home/presentation/committee_actions.dart", "OrderList.Read", "جاهز"),
    ("POST", "api/v1/app/committee/director-notes", "Bearer", "{body}", "204", "ملاحظة للمدير", "Committee", "features/home/presentation/committee_actions.dart", "OrderList.Read", "جاهز"),
    ("POST", "api/v1/app/committee/shifts/start", "Bearer", "—", "StartSalesShiftResponse", "بدء دوام", "Shift", "features/shift/data/shift_remote.dart", "OrderList.Read", "جاهز"),
    ("POST", "api/v1/app/committee/shifts/end", "Bearer", "—", "EndSalesShiftResponse", "إنهاء دوام", "Shift", "features/shift/data/shift_remote.dart", "OrderList.Read", "جاهز"),
    ("GET", "api/v1/app/committee/shifts/current", "Bearer", "—", "GetCurrentSalesShiftResponse", "حالة الدوام", "Shift", "features/shift/data/shift_remote.dart", "OrderList.Read", "جاهز"),
    ("POST", "api/v1/app/committee/locations/batch", "Bearer", "SalesLocationPointDto[]", "IngestBatch", "طابور GPS", "Tracking", "features/shift/data/shift_remote.dart", "OrderList.Read", "جاهز"),
    ("POST", "api/v1/app/committee/locations/live", "Bearer", "{lat,lng}", "IngestLive", "موقع حي", "Tracking", "features/shift/data/shift_remote.dart", "OrderList.Read", "جاهز"),
    ("GET", "api/v1/app/committee/sales-requests", "Bearer", "—", "TBD", "طلبات البيع", "Committee", "features/sales/presentation/sales_requests_page.dart", "Order.Read", "متبقي-API"),
    ("POST", "api/v1/app/committee/sales-requests", "Bearer", "TBD", "TBD", "إرسال طلب مبيع", "Committee", "—", "Order.Create", "متبقي-API"),
    ("GET", "api/v1/app/committee/list-stats", "Bearer", "—", "TBD", "إحصائيات كل القوائم", "Committee", "drawer قريباً", "OrderList.Read", "متبقي-API"),
    ("GET", "api/v1/app/committee/list-stats/{listId}", "Bearer", "—", "TBD", "إحصائيات قائمة", "Committee", "—", "OrderList.Read", "متبقي-API"),
    ("GET", "api/v1/app/committee/legal-followups", "Bearer", "—", "TBD", "زبائن قديمة/قانونية", "Committee", "features/legal/presentation/legal_followups_page.dart", "OrderList.Read", "متبقي-API"),
    ("GET", "api/v1/app/committee/field-followups", "Bearer", "—", "TBD", "متابعات ميدانية", "Committee", "features/field/presentation/field_followups_page.dart", "OrderList.Read", "متبقي-API"),
    ("GET", "api/v1/app/committee/customers/{id}/profile", "Bearer", "listId?", "TBD", "ملف زبون", "Committee", "—", "Customer.Read", "متبقي-API"),
    ("GET", "api/v1/app/committee/collection/customers", "Bearer", "listId", "TBD", "عملاء الجباية", "Collection", "—", "OrderList.Read", "متبقي-API"),
    ("POST", "api/v1/app/committee/collection/post-payment", "Bearer", "TBD", "TBD", "تسجيل تسديد جباية", "Collection", "—", "Order.Update", "متبقي-API"),
    ("GET", "{GATEWAY}/mobile-committee-tasks/{appKey}", "Gateway اختياري", "—", "tasks[]", "مهام اللجنة", "Gateway", "features/tasks/presentation/tasks_page.dart", "Gateway", "اختياري-قلعة"),
]

# NewPath, LegacyFollowers (مرجع بزنس فقط), Notes, Status
MAPPING = [
    ("POST api/v1/app/Auth/login", "POST Followers/Login", "JWT بدل asyncId — API قلعة الضمان", "مكتمل"),
    ("GET api/v1/app/committee/me", "—", "بوابة دور Motaba / مسؤول اللجنة", "مكتمل"),
    ("GET api/v1/app/committee/lists", "GET Followers/Lists", "ملكية MotabaId|MandobId", "مكتمل"),
    ("GET api/v1/app/committee/list-cards", "GET Followers/ListCards", "بطاقات الرئيسية + phantom", "مكتمل"),
    ("GET api/v1/app/committee/daily", "GET Followers/Daily", "showType مسددين/غير مسددين", "مكتمل"),
    ("GET api/v1/app/committee/collection-lists", "GET Followers/CollectionLists", "قوائم الجباية", "مكتمل"),
    ("GET api/v1/app/committee/declining-customers", "GET Followers/DecliningCustomers", "زبائن متدنية", "مكتمل"),
    ("GET api/v1/app/committee/pending-sales", "GET Followers/PendingSales", "مبيعات معلّقة", "مكتمل"),
    ("POST .../pending-sales/{id}/authenticate", "POST Followers/PendingSales/Authenticate", "تصديق مسؤول اللجنة", "مكتمل"),
    ("GET/POST day-ratings*", "Followers/DayRatings*", "تقييم مندوب 0-10", "مكتمل"),
    ("POST complaints", "Followers/complaints", "شكوى", "مكتمل-UI"),
    ("POST director-notes", "Followers/director-notes", "ملاحظة مدير", "مكتمل-UI"),
    ("shifts/* + locations/*", "Followers/shifts|locations*", "دوام+GPS عبر AppArea", "مكتمل"),
    ("sales-requests", "Followers/SalesRequests", "طلبات بيع", "متبقي-API"),
    ("list-stats*", "Followers/ListStats*", "إحصائيات", "متبقي-API"),
    ("legal-followups", "Followers/LegalFollowups*", "قانونية", "متبقي-API"),
    ("field-followups", "Followers/FieldFollowups*", "ميدانية", "متبقي-API"),
    ("customers/{id}/profile", "Followers/Customers/{id}/profile", "بروفايل", "متبقي-API"),
    ("collection/customers|post-payment", "Followers/Collection/*", "جباية تفصيلية", "متبقي-API"),
    ("gateway tasks (اختياري)", "mobile-committee-tasks/follower", "GATEWAY_BASE فارغ افتراضياً — ليس admincompany", "اختياري"),
]

# Action, Screen, API, Status
ACTIONS = [
    ("تسجيل الدخول", "LoginPage", "Auth/login + committee/me", "جاهز"),
    ("بوابة الدوام", "ShiftGatePage", "shifts/current|start", "جاهز"),
    ("إنهاء الدوام", "HomeTab / Drawer", "shifts/end", "جاهز"),
    ("عرض بطاقات القوائم", "HomeTab", "list-cards", "جاهز"),
    ("فتح تفاصيل قائمة", "ListDetailPage", "daily", "جاهز"),
    ("المسددين / غير المسددين", "ListDetailPage", "daily?showType=", "جاهز"),
    ("قوائم الجباية", "CollectionTab", "collection-lists", "جاهز"),
    ("تقييم مندوب ناقص", "CollectionTab", "day-ratings/missing + POST", "جاهز"),
    ("التسديدات (تجميع)", "PaymentsTab", "lists + daily مسددين", "جاهز"),
    ("الزبائن المتدنية", "DecliningTab", "declining-customers", "جاهز"),
    ("المبيعات / تصديق", "PendingSalesPage", "pending-sales + authenticate", "جاهز"),
    ("ملاحظة للمدير", "Home/Drawer → CommitteeActions", "director-notes", "جاهز"),
    ("إرسال شكوى", "Home/Drawer → CommitteeActions", "complaints", "جاهز"),
    ("المهام", "TasksPage", "GATEWAY اختياري قلعة الضمان", "اختياري"),
    ("طلبات البيع", "SalesRequestsPage", "sales-requests", "متبقي-API"),
    ("الإحصائيات", "Drawer قريباً", "list-stats", "متبقي-API"),
    ("الزبائن القديمة", "LegalFollowupsPage", "legal-followups", "متبقي-API"),
    ("ميدانية", "FieldFollowupsPage", "field-followups", "متبقي-API"),
    ("فيديو تعليمي", "Home chip قريباً", "tutorials (قلعة)", "متبقي"),
]


def style_header(ws, headers):
    fill = PatternFill("solid", fgColor="006A64")
    font = Font(color="FFFFFF", bold=True)
    for col, _ in enumerate(headers, 1):
        cell = ws.cell(1, col)
        cell.fill = fill
        cell.font = font
        cell.alignment = Alignment(horizontal="center", wrap_text=True)


def autofit(ws):
    thin = Border(
        left=Side(style="thin"),
        right=Side(style="thin"),
        top=Side(style="thin"),
        bottom=Side(style="thin"),
    )
    for col in ws.columns:
        width = max(len(str(c.value or "")) for c in col) + 2
        ws.column_dimensions[col[0].column_letter].width = min(width, 56)
    for row in ws.iter_rows(min_row=1, max_row=ws.max_row, max_col=ws.max_column):
        for cell in row:
            cell.border = thin
            cell.alignment = Alignment(wrap_text=True, vertical="top")


def write_excel():
    wb = Workbook()
    ws = wb.active
    ws.title = "Endpoints"
    headers = [
        "Method", "Path", "Auth", "Request", "Response", "Purpose", "Area", "ClientFile", "Permission", "Status",
    ]
    ws.append(headers)
    style_header(ws, headers)
    for row in ENDPOINTS:
        ws.append(list(row))
    autofit(ws)

    bases = wb.create_sheet("Bases")
    bases.append(["Name", "Value", "Notes"])
    style_header(bases, ["Name", "Value", "Notes"])
    for r in [
        ("API_BASE", "https://api.qalataldhaman.com/", "dart-define API_BASE — نظام قلعة الضمان"),
        ("Committee prefix", "api/v1/app/committee", "AppArea مسؤول اللجنة"),
        ("Auth prefix", "api/v1/app/Auth", "مشترك JWT"),
        ("GATEWAY_BASE", "(فارغ افتراضياً)", "لا admincompany — اضبط لبوابة قلعة فقط"),
        ("APP_KEY", "committee", "مفتاح مهام الجوال"),
        ("applicationId", "com.qalaat.committee_officer", "Android"),
        ("Role", "Motaba / مسؤول اللجنة", "CommitteeAccess"),
        ("NOT used", "admincompany.alsaaeidy.com", "محظور لهذا التطبيق"),
    ]:
        bases.append(list(r))
    autofit(bases)

    actions = wb.create_sheet("SystemActions")
    actions.append(["Action", "Screen", "API", "Status"])
    style_header(actions, ["Action", "Screen", "API", "Status"])
    for row in ACTIONS:
        actions.append(list(row))
    autofit(actions)

    enums = wb.create_sheet("Enums")
    enums.append(["Name", "Values", "Where"])
    style_header(enums, ["Name", "Values", "Where"])
    for r in [
        ("DailyShowType", "المسددين | الغير مسددين", "daily?showType="),
        ("ProductRoleLabel", "مسؤول اللجنة", "me / UI"),
        ("ShellTabs", "الرئيسية | الجباية | التسديدات | المتدنية", "MainShell"),
        ("DayRatingScore", "0..10", "day-ratings"),
        ("PhantomLabels", "ممتاز|جيد|ضعيف|متلكئ|سيء", "list-cards"),
    ]:
        enums.append(list(r))
    autofit(enums)

    checklist = wb.create_sheet("TestChecklist")
    checklist.append(["#", "Scenario", "Expected", "Pass?"])
    style_header(checklist, ["#", "Scenario", "Expected", "Pass?"])
    for i, r in enumerate(
        [
            ("Login Motaba", "JWT + me.isCommitteeOfficer", ""),
            ("Shift gate", "لا Home بلا دوام", ""),
            ("List cards", "بطاقات من list-cards", ""),
            ("Daily tabs", "مسدد/غير مسدد", ""),
            ("Collection + DayRatings", "تقييم 0-10", ""),
            ("Payments tab", "تجميع مسددين", ""),
            ("Declining", "زبائن متدنية", ""),
            ("Pending sales authenticate", "تصديق طلب", ""),
            ("Complaint", "POST complaints", ""),
            ("Director note", "POST director-notes", ""),
            ("No admincompany call", "GATEWAY فارغ أو قلعة فقط", ""),
        ],
        1,
    ):
        checklist.append([i, r[0], r[1], r[2]])
    autofit(checklist)

    mapping = wb.create_sheet("Mapping")
    mapping.append(["NewPath", "LegacyFollowersBusinessOnly", "Notes", "Status"])
    style_header(mapping, ["NewPath", "LegacyFollowersBusinessOnly", "Notes", "Status"])
    for row in MAPPING:
        mapping.append(list(row))
    autofit(mapping)

    paths = wb.create_sheet("ProjectPaths")
    paths.append(["Path", "Role"])
    style_header(paths, ["Path", "Role"])
    for r in [
        ("lib/main.dart / app.dart", "دخول التطبيق"),
        ("lib/config/app_env.dart", "API_BASE قلعة الضمان"),
        ("lib/features/auth/*", "Login JWT"),
        ("lib/features/shift/*", "دوام + GPS"),
        ("lib/features/home/*", "Shell / Drawer / بطاقات / أكشنات"),
        ("lib/features/lists/*", "تفاصيل قائمة + daily"),
        ("lib/features/collection/*", "جباية + تقييم مندوب"),
        ("lib/features/payments/*", "التسديدات"),
        ("lib/features/declining/*", "المتدنية"),
        ("lib/features/sales/*", "مبيعات معلّقة / طلبات"),
        ("lib/features/legal|field|tasks/*", "قانونية / ميدانية / مهام"),
        ("lib/core/widgets/*", "هوية التصميم الحالية — لا تُمس في نسخ الأكشنات"),
        ("../api-main/.../CommitteeController.cs", "Backend AppArea"),
        ("docs/CommitteeOfficer_*.docx|xlsx", "هذا التوثيق"),
    ]:
        paths.append(list(r))
    autofit(paths)

    out = ROOT / "CommitteeOfficer_API_Endpoints.xlsx"
    wb.save(out)
    print("Wrote", out)


def write_word():
    doc = Document()
    title = doc.add_heading("مسؤول اللجنة — مسارات المشروع والعمارة وأكشنات النظام", 0)
    title.alignment = WD_ALIGN_PARAGRAPH.RIGHT

    def p(text):
        para = doc.add_paragraph(text)
        para.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        for run in para.runs:
            run.font.size = Pt(12)
            run.font.name = "Arial"
        return para

    doc.add_heading("1) نطاق المنتج", level=1)
    p("تطبيق مسؤول اللجنة داخل منظومة قلعة الضمان (accountant_qlt_system).")
    p("البزنس/الأكشنات: نفس عمل نظام المتابع (قوائم، يومي، جباية، تسديدات، متدنية، تصديق مبيعات، دوام، شكوى، ملاحظة مدير).")
    p("الـ API والتصميم: خاصان بهذا المشروع — Base = api.qalataldhaman.com — ممنوع الاعتماد على admincompany.alsaaeidy.com.")
    p("التصميم الحالي (waves / glassy bar / centered dialogs) لا يُعاد تصميمه عند نسخ الأكشنات.")

    doc.add_heading("2) مسارات المشروع", level=1)
    p(r"Flutter: C:\GitHub\accountant_qlt_system\committee_officer_application")
    p("applicationId: com.qalaat.committee_officer")
    p(r"Backend: api-main\src\Api\Controllers\V1\AppArea\CommitteeController.cs")
    p(r"Handlers: api-main\src\Application\Committee\*")
    p("مرجع البزنس فقط (لا استدعاء شبكي): follower_application في SalesHaider")
    p("تفاصيل المسارات البرمجية: ورقة ProjectPaths في ملف Excel")

    doc.add_heading("3) Architecture", level=1)
    p(
        "UI (MainShell + Tabs + Drawer) → CommitteeRemote / AuthRemote / ShiftRemote (Dio + JWT) "
        "→ api/v1/app/Auth + api/v1/app/committee → MediatR → DB (OrderLists.MotabaId)."
    )
    p("Gateway مهام اختياري عبر GATEWAY_BASE لنفس منظومة قلعة الضمان فقط؛ الافتراضي فارغ.")
    p(
        "Mermaid:\n"
        "App → Remotes → API_BASE(qalataldhaman) → CommitteeController → Handlers → Company DB\n"
        "App → (optional GATEWAY) → mobile-committee-tasks/committee"
    )

    doc.add_heading("4) أكشنات النظام (منسوخة من بزنس المتابع)", level=1)
    p("دخول JWT → committee/me → بوابة دوام → الرئيسية/بطاقات القوائم.")
    p("تفاصيل قائمة: daily مسددين/غير مسددين.")
    p("الجباية: collection-lists + تقييم مندوب day-ratings قبل الإغلاق التشغيلي.")
    p("التسديدات: تجميع المسددين عبر القوائم.")
    p("المتدنية: declining-customers.")
    p("المبيعات: pending-sales + authenticate.")
    p("شكوى / ملاحظة مدير: complaints + director-notes (مربوطة في الواجهة).")
    p("متبقي على الـ API: طلبات بيع، إحصائيات، قانونية، ميدانية، بروفايل زبون، جباية تفصيلية/دفع.")

    doc.add_heading("5) ما لا يُنسخ", level=1)
    p("لا تُنسخ روابط Followers/* ولا admincompany ولا asyncId كاعتماد أساسي.")
    p("لا يُغيَّر تصميم الشاشات الحالية؛ فقط ربط الأكشنات والـ docs.")

    doc.add_heading("6) توليد هذه الملفات", level=1)
    p("python docs/generate_docs.py من جذر committee_officer_application")

    out = ROOT / "CommitteeOfficer_Project_Paths_and_Architecture.docx"
    doc.save(out)
    print("Wrote", out)


if __name__ == "__main__":
    write_excel()
    write_word()
