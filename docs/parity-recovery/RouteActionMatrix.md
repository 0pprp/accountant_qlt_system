# RouteActionMatrix

مفتاح ثابت لكل رحلة. الحالة هنا = جرد كود، وليست Device/Browser-live.

| StableKey | Legacy | New | Status | Priority |
|---|---|---|---|---|
| `FE.home` | FE_Company/navigation/vertical/index.js:3 · الرئيسية | PANEL.home · navigation.registry.ts:20 | Mapped | — |
| `FE.director.ratings` | index.js:8 · التقييمات | PANEL.management.ratings · /management/ratings | Mapped | — |
| `FE.director.summary` | index.js:13 · الملخص | PANEL.management.summary · /management/summary | Mapped | — |
| `FE.director.delegate-followup` | index.js:18-25 · متابعة المندوبين | PANEL.management.delegateFollowup · /management/director/delegate-followup | Mapped | — |
| `FE.director.sales-followup` | index.js:28-35 · متابعة المبيعات | PANEL.sales.followup · /sale/followup | Mapped | — |
| `FE.director.follower-work.sales` | index.js:38-47 · عمل المتابعين/مصادقة | PANEL.management.followerWork.sales · .../follower-work/sales | Mapped | — |
| `FE.director.follower-work.declining` | index.js:48-56 · المتدنية | PANEL.management.followerWork.declining · .../declining | Mapped | — |
| `FE.director.follower-work.notes` | index.js:57-65 · الملاحظات | PANEL.management.followerWork.notes · .../notes | Mapped | — |
| `FE.director.follower-work.old-customers` | index.js:66-74 · الزبائن القديمة | PANEL.management.followerWork.oldCustomers · .../old-customers | Mapped | — |
| `FE.director.follower-work.delegate-notes` | index.js:75-84 · ملاحظات المندوبين | PANEL.management.followerWork.delegateNotes · .../delegate-notes | Mapped | — |
| `FE.director.follower-work.collection` | index.js:85-93 · الجباية | PANEL.management.followerWork.collection · .../collection | Mapped | — |
| `FE.monitor.followers-map` | index.js:94-99 · الموقع المباشر متابعين | PANEL.management.followerWork.liveMap · .../live-map | Mapped | — |
| `FE.monitor.followers-routes` | index.js:100-104 · مسارات متابعين | PANEL.management.followerWork.routes · .../routes | Mapped | — |
| `FE.sales-manager.requests` | index.js:107-111 · طلبات البيع | PANEL.sales.requests · /sale/requests | Mapped | — |
| `FE.director.tutorial-videos` | index.js:112-116 · فيديوهات | PANEL.management.tutorials · /management/oversight/tutorials | Mapped | — |
| `FE.director.announcements` | index.js:117-121 · إعلانات | PANEL.management.announcements · .../announcements | Mapped | — |
| `FE.director.assigned-tasks` | index.js:122-126 · المهام | PANEL.management.*tasks · branch/delegate assigned-tasks | Mapped | — |
| `FE.director.branch-visits` | index.js:127-131 · أعمال إدارة الفرع | PANEL.management.branchVisits · delegate/branch visits tabs | Mapped | — |
| `FE.director.committee-tasks` | index.js:132-136 · مهام اللجنة | PANEL.management.committeeTasks · /management/oversight/committee-tasks | Mapped | — |
| `FE.director.notes` | index.js:137-141 · ملاحظات التطبيقات | PANEL.management.appNotes · .../app-notes | Mapped | — |
| `FE.director.waivers` | index.js:142-146 · التنازلات | PANEL.management.waivers · oversight/branch/delegate waivers | Mapped | — |
| `FE.users.list` | index.js:148-157 · المستخدمين | PANEL.people.users · /user | Mapped | — |
| `FE.users.activity` | index.js:158-162 · نشاطات | PANEL.system.activityLog · /activity-log | Mapped | — |
| `FE.inventory.*` | index.js:163-182 · مخازن/عناصر/موردين/مشتريات | PANEL.inventory/finance · warehouse+purchases | Partial | P1 |
| `FE.customers.*` | index.js:184-212 · عملاء | PANEL.operations.customer · /customer (+audit) | Mapped | — |
| `FE.daily-followup` | index.js:214-218 · المتابعة اليومية | — · — | Gap | P2 |
| `FE.price-board` | index.js:219-223 · لوحة الأسعار | PANEL.management.priceBoard · .../price-board | Mapped | — |
| `FE.legal.*` | index.js:225-242 · قانونية | PANEL.management.legalNames+decisions+waivers · management/branch | Mapped | — |
| `FE.sales.list` | index.js:244-248 · المبيعات | PANEL.sales / Sale · /sale | Mapped | — |
| `FE.sales-manager.dashboard` | index.js:250-256 · نظرة عامة | PANEL.sales.overview · /sale/overview | Mapped | — |
| `FE.sales-manager.employees` | line~255 · موظفين | PANEL.sales.employees · /sale/employees | Mapped | — |
| `FE.sales-manager.map` | line~256 · موقع مبيعات | PANEL.sales.liveMap · /sale/live-map | Mapped | — |
| `FE.sales-manager.routes` | line~257 · مسارات | PANEL.sales.routes · /sale/routes | Mapped | — |
| `FE.sales-manager.sales` | line~258 · مبيعات SM | PANEL.sales.completed · /sale/completed | Mapped | — |
| `FE.sales-manager.sales-summary` | line~259 · ملخص | PANEL.sales.summary · /sale/summary | Mapped | — |
| `FE.sales-manager.legal-followups` | line~260 · قانونية مبيعات | PANEL.sales.legal · /sale/legal | Mapped | — |
| `FE.sales-manager.excel-search` | sales-manager-excel-search · بحث أسماء | PANEL.sales.excelSearch · /sale/excel-search | Mapped (09) | Unit |
| `FE.sales-manager.purchases` | sales-manager-purchases · مشتريات | PANEL.finance.purchases · /purchases | Mapped (finance parent) | مقصود خارج sale |
| `FE.payments.*` | index.js:266-292 · تسديدات | PANEL.operations.payment · /payment | Mapped | — |
| `FE.delegates.*` | index.js:298-319 · مندوبين/إحصائيات | PANEL partial · users+reports? | Gap | P1 |
| `FE.finance.*` | index.js:324-345 · خزائن | PANEL.finance.safe* · /safe | Mapped | — |
| `FE.backup.database` | index.js:347-350 · نسخ احتياطي | — · ops/scripts | N/A | — |
| `LEGACY.delegate.home` | delegate_application/HomePage.dart · bottom+drawer | DELEGATE.home.* · delegate_app home_shell | Mapped | — |
| `LEGACY.delegate.sales-requests` | HomePage drawer · طلبات البيع | DELEGATE.drawer.salesRequests · SubmittedSalesRequestsPage | Mapped | — |
| `LEGACY.follower.shell` | follower_home_shell.dart · tabs+drawer | COMMITTEE.home.* + drawer · committee_officer_application | Mapped | — |
| `LEGACY.follower.statistics` | follower_home_shell.dart:133 · الإحصائيات | COMMITTEE.drawer.statistics · ComingSoon only | Gap | P1 |
| `LEGACY.follower.field-followups` | field_followups_page.dart live API · ميدانية | COMMITTEE.drawer.fieldFollowups · ComingSoonPage | Gap | P0 |
| `LEGACY.sales.bins` | pending_sales_screen.dart · ستة صناديق | SALES.home.requests + bins · sales_employee_app | Mapped | — |
| `LEGACY.sales.transfer-name` | pending_sales_screen transferSalesRequestName · نقل الاسم | SALES.bin.reassign · bin_orders_page live unbound | Gap | P0 |
| `LEGACY.sales.drawer.archive+` | sales pending drawers · أرشيف/فيديو/شكوى/ملاحظة | SALES.drawer.* · home_drawer ComingSoon×4 | Gap | P0 |
| `LEGACY.branch.shell` | branch_manager home_shell.dart · 10 tabs | PANEL.managementBranch APP_TABS · workspaceNavigation.ts:11-21 | Mapped | — |
| `LEGACY.delegated.shell` | delegated_manager home_shell.dart · 10 tabs | PANEL.managementDelegate APP_TABS · workspaceNavigation.ts:23-33 | Mapped | — |
| `LEGACY.main-follower.*` | main_follower_application · missions/city hub | — · — | N/A | P2 |
| `LEGACY.mobile_app.*` | mobile_app home_shell · ERP bottom+drawer | APPDEV.home.* · app-dev MainPage | Partial | P1 |
| `NEW.only.committee-ratings` | — · — | sections committee-ratings · not_implemented | Declared | P2 |

## Actions (عيّنة تتبع زر→API)

| Key | Button | Condition | API | Compare |
|---|---|---|---|---|
| `PANEL.sales.requests` | تدقيق/فتح طلب | tab permission + selected | GET admin SalesRequests* | fields tracked — not Device |
| `SALES.bin.reassign` | نقل الاسم | order actions sheet | POST transfer (legacy) / demo local | Gap P0 live unbound |
| `COMMITTEE.list.delegateNote` | ملاحظة مندوب | list selected + write perm | POST .../delegate-notes | API wired — Device open |
| `COMMITTEE.drawer.fieldFollowups` | ميدانية | drawer always shown | expected committee/field-followups | Gap P0 placeholder |
| `SALES.drawer.tutorials` | فيديوهات | ComingSoon | Tutorial videos app API exists elsewhere | Gap — no sales page yet |