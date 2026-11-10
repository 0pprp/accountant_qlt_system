# -*- coding: utf-8 -*-
from pathlib import Path
from openpyxl import Workbook
from openpyxl.styles import Font, Alignment, PatternFill
from docx import Document
from docx.shared import Pt

root = Path(__file__).resolve().parent
root.mkdir(parents=True, exist_ok=True)

rows = [
    ("FE.home", "FE_Company/navigation/vertical/index.js:3", "الرئيسية", "PANEL.home", "navigation.registry.ts:20", "overview", "Mapped", "—", "Dashboard"),
    ("FE.director.ratings", "index.js:8", "التقييمات", "PANEL.management.ratings", "/management/ratings", "oversight", "Mapped", "—", "ManagementRatings.Read"),
    ("FE.director.summary", "index.js:13", "الملخص", "PANEL.management.summary", "/management/summary", "oversight", "Mapped", "—", ""),
    ("FE.director.delegate-followup", "index.js:18-25", "متابعة المندوبين", "PANEL.management.delegateFollowup", "/management/director/delegate-followup", "oversight-delegates", "Mapped", "—", "ليست متابعة موظفي المبيعات"),
    ("FE.director.sales-followup", "index.js:28-35", "متابعة المبيعات", "PANEL.sales.followup", "/sale/followup", "sales-employees", "Mapped", "—", "SellerId — منفصل عن المندوبين"),
    ("FE.director.follower-work.sales", "index.js:38-47", "عمل المتابعين/مصادقة", "PANEL.management.followerWork.sales", ".../follower-work/sales", "follower-work", "Mapped", "—", ""),
    ("FE.director.follower-work.declining", "index.js:48-56", "المتدنية", "PANEL.management.followerWork.declining", ".../declining", "follower-work", "Mapped", "—", ""),
    ("FE.director.follower-work.notes", "index.js:57-65", "الملاحظات", "PANEL.management.followerWork.notes", ".../notes", "follower-work", "Mapped", "—", ""),
    ("FE.director.follower-work.old-customers", "index.js:66-74", "الزبائن القديمة", "PANEL.management.followerWork.oldCustomers", ".../old-customers", "follower-work", "Mapped", "—", ""),
    ("FE.director.follower-work.delegate-notes", "index.js:75-84", "ملاحظات المندوبين", "PANEL.management.followerWork.delegateNotes", ".../delegate-notes", "follower-work", "Mapped", "—", ""),
    ("FE.director.follower-work.collection", "index.js:85-93", "الجباية", "PANEL.management.followerWork.collection", ".../collection", "follower-work", "Mapped", "—", ""),
    ("FE.monitor.followers-map", "index.js:94-99", "الموقع المباشر متابعين", "PANEL.management.followerWork.liveMap", ".../live-map", "follower-work", "Mapped", "—", "ActorKind follower"),
    ("FE.monitor.followers-routes", "index.js:100-104", "مسارات متابعين", "PANEL.management.followerWork.routes", ".../routes", "follower-work", "Mapped", "—", ""),
    ("FE.sales-manager.requests", "index.js:107-111", "طلبات البيع", "PANEL.sales.requests", "/sale/requests", "sales-requests", "Mapped", "—", "Workspace tabs"),
    ("FE.director.tutorial-videos", "index.js:112-116", "فيديوهات", "PANEL.management.tutorials", "/management/oversight/tutorials", "oversight", "Mapped", "—", ""),
    ("FE.director.announcements", "index.js:117-121", "إعلانات", "PANEL.management.announcements", ".../announcements", "oversight", "Mapped", "—", ""),
    ("FE.director.assigned-tasks", "index.js:122-126", "المهام", "PANEL.management.*tasks", "branch/delegate assigned-tasks", "mgmt-tasks", "Mapped", "—", "ليست مهام اللجنة"),
    ("FE.director.branch-visits", "index.js:127-131", "أعمال إدارة الفرع", "PANEL.management.branchVisits", "delegate/branch visits tabs", "branch-ops", "Mapped", "—", ""),
    ("FE.director.committee-tasks", "index.js:132-136", "مهام اللجنة", "PANEL.management.committeeTasks", "/management/oversight/committee-tasks", "committee-tasks", "Mapped", "—", ""),
    ("FE.director.notes", "index.js:137-141", "ملاحظات التطبيقات", "PANEL.management.appNotes", ".../app-notes", "oversight", "Mapped", "—", ""),
    ("FE.director.waivers", "index.js:142-146", "التنازلات", "PANEL.management.waivers", "oversight/branch/delegate waivers", "waivers", "Mapped", "—", ""),
    ("FE.users.list", "index.js:148-157", "المستخدمين", "PANEL.people.users", "/user", "admin", "Mapped", "—", ""),
    ("FE.users.activity", "index.js:158-162", "نشاطات", "PANEL.system.activityLog", "/activity-log", "admin", "Mapped", "—", ""),
    ("FE.inventory.*", "index.js:163-182", "مخازن/عناصر/موردين/مشتريات", "PANEL.inventory/finance", "warehouse+purchases", "inventory", "Partial", "P1", "suppliers path TBD if missing module"),
    ("FE.customers.*", "index.js:184-212", "عملاء", "PANEL.operations.customer", "/customer (+audit)", "customers", "Mapped", "—", "archived/stopped may be partial"),
    ("FE.daily-followup", "index.js:214-218", "المتابعة اليومية", "—", "—", "admin", "Gap", "P2", "لا مسار لوحة مكافئ معلن"),
    ("FE.price-board", "index.js:219-223", "لوحة الأسعار", "PANEL.management.priceBoard", ".../price-board", "oversight", "Mapped", "—", ""),
    ("FE.legal.*", "index.js:225-242", "قانونية", "PANEL.management.legalNames+decisions+waivers", "management/branch", "legal", "Mapped", "—", ""),
    ("FE.sales.list", "index.js:244-248", "المبيعات", "PANEL.sales / Sale", "/sale", "sales", "Mapped", "—", ""),
    ("FE.sales-manager.dashboard", "index.js:250-256", "نظرة عامة", "PANEL.sales.overview", "/sale/overview", "sales", "Mapped", "—", ""),
    ("FE.sales-manager.employees", "line~255", "موظفين", "PANEL.sales.employees", "/sale/employees", "sales", "Mapped", "—", ""),
    ("FE.sales-manager.map", "line~256", "موقع مبيعات", "PANEL.sales.liveMap", "/sale/live-map", "sales", "Mapped", "—", "SalesEmployee actor"),
    ("FE.sales-manager.routes", "line~257", "مسارات", "PANEL.sales.routes", "/sale/routes", "sales", "Mapped", "—", ""),
    ("FE.sales-manager.sales", "line~258", "مبيعات SM", "PANEL.sales.completed", "/sale/completed", "sales", "Mapped", "—", ""),
    ("FE.sales-manager.sales-summary", "line~259", "ملخص", "PANEL.sales.summary", "/sale/summary", "sales", "Mapped", "—", ""),
    ("FE.sales-manager.legal-followups", "line~260", "قانونية مبيعات", "PANEL.sales.legal", "/sale/legal", "sales", "Mapped", "—", ""),
    ("FE.payments.*", "index.js:266-292", "تسديدات", "PANEL.operations.payment", "/payment", "payments", "Mapped", "—", "week/month filters may be UI gaps"),
    ("FE.delegates.*", "index.js:298-319", "مندوبين/إحصائيات", "PANEL partial", "users+reports?", "delegates-admin", "Gap", "P1", "إحصائيات مع/بدون مصفرين غير مربوطة"),
    ("FE.finance.*", "index.js:324-345", "خزائن", "PANEL.finance.safe*", "/safe", "finance", "Mapped", "—", ""),
    ("FE.backup.database", "index.js:347-350", "نسخ احتياطي", "—", "ops/scripts", "ops", "N/A", "—", "تشغيل خادم لا واجهة مستخدم"),
    ("LEGACY.delegate.home", "delegate_application/HomePage.dart", "bottom+drawer", "DELEGATE.home.*", "delegate_app home_shell", "delegate", "Mapped", "—", ""),
    ("LEGACY.delegate.sales-requests", "HomePage drawer", "طلبات البيع", "DELEGATE.drawer.salesRequests", "SubmittedSalesRequestsPage", "delegate", "Mapped", "—", "originator inbox"),
    ("LEGACY.follower.shell", "follower_home_shell.dart", "tabs+drawer", "COMMITTEE.home.* + drawer", "committee_officer_application", "committee", "Mapped", "—", ""),
    ("LEGACY.follower.statistics", "follower_home_shell.dart:133", "الإحصائيات", "COMMITTEE.drawer.statistics", "ComingSoon only", "committee", "Gap", "P1", "مدخل ظاهر بلا API"),
    ("LEGACY.follower.field-followups", "field_followups_page.dart live API", "ميدانية", "COMMITTEE.drawer.fieldFollowups", "ComingSoonPage", "committee", "Gap", "P0", "لا تحذف المدخل"),
    ("LEGACY.sales.bins", "pending_sales_screen.dart", "ستة صناديق", "SALES.home.requests + bins", "sales_employee_app", "sales-emp", "Mapped", "—", ""),
    ("LEGACY.sales.transfer-name", "pending_sales_screen transferSalesRequestName", "نقل الاسم", "SALES.bin.reassign", "bin_orders_page live unbound", "sales-emp", "Gap", "P0", "demo id<0 only"),
    ("LEGACY.sales.drawer.archive+", "sales pending drawers", "أرشيف/فيديو/شكوى/ملاحظة", "SALES.drawer.*", "home_drawer ComingSoon×4", "sales-emp", "Gap", "P0", "أربعة مداخل انتظار"),
    ("LEGACY.branch.shell", "branch_manager home_shell.dart", "10 tabs", "PANEL.managementBranch APP_TABS", "workspaceNavigation.ts:11-21", "branch", "Mapped", "—", ""),
    ("LEGACY.delegated.shell", "delegated_manager home_shell.dart", "10 tabs", "PANEL.managementDelegate APP_TABS", "workspaceNavigation.ts:23-33", "delegate-mgr", "Mapped", "—", ""),
    ("LEGACY.main-follower.*", "main_follower_application", "missions/city hub", "—", "—", "main-follower", "N/A", "P2", "لا تطبيق جديد مكافئ"),
    ("LEGACY.mobile_app.*", "mobile_app home_shell", "ERP bottom+drawer", "APPDEV.home.*", "app-dev MainPage", "erp-mobile", "Partial", "P1", "هوية app-dev من الكود"),
    ("NEW.only.committee-ratings", "—", "—", "sections committee-ratings", "not_implemented", "oversight", "Declared", "P2", "غرض معلن بلا مسار حي"),
]

wb = Workbook()
ws = wb.active
ws.title = "SourceParity"
headers = ["StableKey", "LegacySource", "LegacyEntry", "NewConsumer", "NewEntry", "Journey", "Status", "Priority", "Notes", "ProofClass"]
ws.append(headers)
for r in rows:
    ws.append(list(r) + ["Implemented-inventory"])
for cell in ws[1]:
    cell.font = Font(bold=True, color="FFFFFF")
    cell.fill = PatternFill("solid", fgColor="006A64")
    cell.alignment = Alignment(wrap_text=True, vertical="top")
for col in ws.columns:
    ws.column_dimensions[col[0].column_letter].width = 22

ws2 = wb.create_sheet("Actions")
ws2.append(["StableKey", "VisibleButton", "Condition", "Writer", "Reader", "API(new)", "DBEffect", "OtherUI", "Auth", "BranchScope", "CompareResult"])
actions = [
    ("PANEL.sales.requests", "تدقيق/فتح طلب", "tab permission + selected", "SalesRequestsWorkspace", "RequestCard", "GET admin SalesRequests*", "read", "MatchingSummary", "SalesRequest.*", "branch claims", "fields tracked — not Device"),
    ("SALES.bin.reassign", "نقل الاسم", "order actions sheet", "bin_orders_page", "peers API missing for live", "POST transfer (legacy) / demo local", "demo only", "—", "session", "—", "Gap P0 live unbound"),
    ("COMMITTEE.list.delegateNote", "ملاحظة مندوب", "list selected + write perm", "DelegateNoteButton", "committee remote", "POST .../delegate-notes", "FollowerDelegateNotes", "panel follower notes", "FollowerWork.DelegateNotesWrite", "Motaba assignment", "API wired — Device open"),
    ("COMMITTEE.drawer.fieldFollowups", "ميدانية", "drawer always shown", "ComingSoonPage", "—", "expected committee/field-followups", "none", "—", "session", "—", "Gap P0 placeholder"),
    ("SALES.drawer.tutorials", "فيديوهات", "ComingSoon", "—", "—", "Tutorial videos app API exists elsewhere", "none", "delegate has page", "—", "—", "Gap — no sales page yet"),
]
for a in actions:
    ws2.append(list(a))
for cell in ws2[1]:
    cell.font = Font(bold=True, color="FFFFFF")
    cell.fill = PatternFill("solid", fgColor="006A64")

ws3 = wb.create_sheet("Priorities")
ws3.append(["Priority", "Item", "Location", "ChangePlace", "BlockedBy"])
for p in [
    ("P0", "field_followups ComingSoon", "committee_officer .../field_followups_page.dart", "wire committee field API + UI", "endpoint/contract"),
    ("P0", "نقل الاسم الحي", "sales_employee_app .../bin_orders_page.dart", "peers list + transfer command", "API peers"),
    ("P0", "أربعة مداخل sales drawer قريباً", "home_drawer.dart 164-199", "صفحات tutorials/complaint/note/archive", "feature pages"),
    ("P1", "إحصائيات لجنة", "app_drawer.dart:104", "ListStats equivalent", "API"),
    ("P1", "عزل DemoOrdersStore عن المسار الحي", "injection/bin/wizard", "feature flag / remove live merge", "tests"),
    ("P1", "إحصائيات مندوبي FE", "FE.delegates.statistics*", "panel report or N/A product", "product"),
    ("P2", "main_follower N/A consumer", "legacy main_follower_application", "product decision", "scope"),
    ("P2", "committee-ratings not_implemented", "Management/sections.ts", "implement or keep declared", "product"),
    ("P2", "FE.daily-followup", "legacy daily-followup", "map or N/A", "product"),
]:
    ws3.append(list(p))

ws4 = wb.create_sheet("FileMap")
ws4.append(["Area", "Path", "Role"])
for f in [
    ("Legacy FE nav", "SalesHaider-main/.../FE_Company/src/navigation/vertical/index.js", "source menu"),
    ("New panel nav", "panel-admin-main/.../Core/navigation/navigation.registry.ts", "consumer registry"),
    ("Management tabs", ".../Management/domain/sections.ts", "tab contracts"),
    ("App shells map", ".../Management/domain/workspaceNavigation.ts", "branch/delegate order"),
    ("Delegate shell", "delegate_app/lib/features/home/presentation/home_shell.dart", "Flutter nav"),
    ("Sales drawer", "sales_employee_app/.../home_drawer.dart", "gaps×4"),
    ("Committee drawer", "committee_officer_application/.../app_drawer.dart", "stats soon"),
    ("Sales card", "sales_employee_app/.../sales_request_card.dart", "sample card"),
    ("Panel RequestCard", "panel-admin-main/.../SalesRequests/components/RequestCard.vue", "sample card"),
    ("MatchingSummary", ".../MatchingSummary.vue", "match grid 2×2/4"),
    ("Theme Vue", ".../assets/styles/theme.css", "tokens"),
    ("Theme Flutter", "*/core/theme/app_colors.dart", "#006A64 #F4F7F7"),
]:
    ws4.append(list(f))

wb.save(root / "Parity_Matrix.xlsx")
wb.save(root / "RouteActionMatrix.xlsx")

md = [
    "# RouteActionMatrix",
    "",
    "مفتاح ثابت لكل رحلة. الحالة هنا = جرد كود، وليست Device/Browser-live.",
    "",
    "| StableKey | Legacy | New | Status | Priority |",
    "|---|---|---|---|---|",
]
for r in rows:
    md.append(f"| `{r[0]}` | {r[1]} · {r[2]} | {r[3]} · {r[4]} | {r[6]} | {r[7]} |")
md += [
    "",
    "## Actions (عيّنة تتبع زر→API)",
    "",
    "| Key | Button | Condition | API | Compare |",
    "|---|---|---|---|---|",
]
for a in actions:
    md.append(f"| `{a[0]}` | {a[1]} | {a[2]} | {a[5]} | {a[10]} |")
(root / "RouteActionMatrix.md").write_text("\n".join(md), encoding="utf-8")

cv = """# CurrentVsLegacy

## مبدأ الفصل
- أعمال المدير/المراقب (FE director/monitor) → لوحة Management/oversight + Sales حسب الصلاحية.
- تطبيق الفرع والمفوض → Management branch/delegate APP_TABS (ترتيب المصدر القديم).
- متابعة المندوبين ≠ متابعة موظفي المبيعات.
- مهام الإدارة (Task) ≠ مهام اللجنة (CommitteeTask) ≠ عمل المتابعين (FollowerWork).
- الأدوار قوالب بدء؛ التفويض النهائي بالعمليات والصلاحيات على السيرفر.

## جدول مطابقة مختصر
انظر Parity_Matrix.xlsx وRouteActionMatrix.md.

## N/A المبرر
| مصدر | السبب |
|---|---|
| main_follower_application | مهام متعددة المحافظات بلا مستهلك Flutter جديد؛ دمجها في committee_officer يخلط الهويتين |
| MA_ShortCompany | غلاف WebView لموقع خارجي |
| FE.backup.database | تشغيل خادم/نسخ؛ ليس شاشة مستخدم في الجديد |
| committee-ratings (جديد معلن) | not_implemented عن قصد حتى قرار منتج |

## اختلافات مقصودة (مشدّدة أمنياً)
| موضوع | قديم | جديد | سبب |
|---|---|---|---|
| صلاحيات | أدوار ثابتة في الراوتر أحياناً | AccessRequirement على الموارد | لا Admin bypass بالاسم |
| نطاق الفرع | غالباً من جلسة/هاردكود | claims + UserBranches في الـhandler | لا فرع من payload |
| Demo | مسارات demo واسعة | يجب ألا يكون fallback عند فشل API | عزل تدريجي؛ sales ما زال فيه DemoOrdersStore — فجوة P1 |

## مداخل مكررة (ليست أخطاء)
- طلبات البيع في تبويب المبيعات وفي workspace الإدارة: نفس البزنس بمداخل صلاحية مختلفة.
- طلبات البيع في bottom nav موظف المبيعات وفي الدرج: مدخل مزدوج متعمد.

## روابط خاطئة مؤكدة في هذه الدفعة
- لا اكتشاف رابط يوجّه لمتابعة مندوبين بدل موظفين أو العكس في registry الحالي (ملاحظات صريحة في navigation.registry).
- التصحيح هنا: توثيق الفجوات؛ لا حذف مداخل ComingSoon.
"""
(root / "CurrentVsLegacy.md").write_text(cv, encoding="utf-8")

prio = """# Priority change list (01)

## P0 — أماكن التغيير
1. `committee_officer_application/lib/features/field/presentation/field_followups_page.dart` — استبدال ComingSoon بمسار FieldFollowups الحقيقي (عقد API جديد يُوثَّق كجديد إن لزم).
2. `sales_employee_app/lib/features/sales/presentation/bin_orders_page.dart` (107–120) — ربط نقل الاسم الحي بواجهة الزملاء/الأمر؛ الإبقاء على الشرط الأمني.
3. `sales_employee_app/lib/features/home/presentation/widgets/home_drawer.dart` (164–199) — أربع صفحات: أرشيف، فيديوهات، شكوى، ملاحظة مدير (إعادة استخدام APIs الموجودة حيث تتوفر).

## P1
4. `committee_officer_application/.../app_drawer.dart:104` — إحصائيات.
5. `sales_employee_app/.../demo_orders_store.dart` + injection + استدعاءات المسار الحي — عزل demo عن الإنتاج.
6. إحصائيات مندوبي FE — تقرير لوحة أو N/A منتج.

## P2
7. قرار منتج لـ main_follower_application.
8. `Management/domain/sections.ts` committee-ratings.
9. FE.daily-followup.

لا تبدأ تغييرات حسابية/مخطط في دفعة 01.
"""
(root / "Priority_P0_P1_P2.md").write_text(prio, encoding="utf-8")

doc = Document()
style = doc.styles["Normal"]
style.font.name = "Calibri"
style.font.size = Pt(11)
doc.add_heading("Parity Recovery — Architecture", level=1)
doc.add_paragraph("دفعة 01–02. لا نشر إنتاج. العقود الجديدة تُعلَّم صراحة.")
doc.add_heading("Endpoints (عيّنة موثّقة للمستهلكين الحاليين)", level=2)
doc.add_paragraph(
    "GET /api/v1/app/SalesRequests/submitted/{source}?tab=&page= — auth: Submit permission + session author + branch — consumers: delegate_app, committee_officer submitted inbox."
)
doc.add_paragraph(
    "POST /api/v1/app/committee/order-lists/{listId}/delegate-notes — auth: FollowerDelegateNotes.Write + Motaba — consumer: DelegateNoteButton."
)
doc.add_paragraph("GET /api/v1/app/mobile-updates/{appKey} — anonymous check — consumers: mobile apps.")
doc.add_paragraph("عقود field-followups / live name transfer peers: غير مغلقة كـAPI-live في 01؛ تُسجَّل فجوة لا اختراع.")
doc.add_heading("FileMap", level=2)
doc.add_paragraph("انظر ورقة FileMap في Parity_Matrix.xlsx.")
doc.add_heading("Actions / SourceParity / Acceptance / OpenGaps", level=2)
doc.add_paragraph("SourceParity: RouteActionMatrix.md + Excel.")
doc.add_paragraph("Acceptance دفعة 01: كل تنقل قديم له مقابل أو فجوة معلنة؛ لا شاشة مطابقة بلا تتبع.")
doc.add_paragraph("OpenGaps: Priority_P0_P1_P2.md.")
doc.add_heading("UI Kit (دفعة 02)", level=2)
doc.add_paragraph("Vue: Core/components/ui/* + /dev/ui-kit preview (DEV).")
doc.add_paragraph("Flutter: core/widgets/ui/* في التطبيقات العيّنة.")
doc.add_paragraph("Tokens: primary #006A64, ink #00201E, card white, search #F8F8F8, Flutter bg #F4F7F7, ElMessiri.")
doc.save(root / "Architecture.docx")
print("wrote", root)
