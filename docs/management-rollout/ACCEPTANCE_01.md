# قبول المرحلة 01 — هيكل الإدارة وربط الموجود

إغلاق محلي بتاريخ 2026-11-08 على بيئة معزولة (`API :4200` + `Vite :5173`) بحسابي `admin` و`limited01`. لا نشر إنتاج.

الدليل: `docs/management-rollout/evidence/01/` · الملفات الرسمية: `Management_Endpoints.xlsx` · `Management_Architecture.docx`

## Implemented

| عنصر | دليل |
|------|------|
| أيقونة مكبرة RequestFilters (خلفية `#F8F8F8`) | `RequestFilters.vue` + `admin-search-icon.png` |
| سجل أقسام/تبويبات مركزي | `modules/Management/domain/sections.ts` |
| ManagementWorkspace + router | `ManagementWorkspace.vue`, `router.ts` |
| إعادة استخدام الصفحات دون نسخ منطق | loaders في `Management/router.ts` |
| شروط route غير مضعّفة | نفس `accessRequirement` لمسارات المصدر |
| توسعة SideBar بمفتاح `section.id` + `aria-controls` فريد | `TheSideBar.vue` |
| استثناءات المفوض + منع موافقة المقدم | API + رسالة UI |
| تسمية تقييم موظفي المبيعات | تبويب `salesEmployeeRating` |
| إدارة حسابات عبر ManagePermissions | `accounts` = Read ∧ ManagePermissions |
| تبويبات غير منفّذة موثّقة فقط | `not_implemented` — لا routes فعّالة |
| عزل الفرع: `branchAbort` + ختم `branchGeneration` في axios + `cancelQueries`/`clear` | `branchAbort.ts`, `axios.ts`, `vue-query.ts` |
| مفتاح إعادة التركيب على `branchGeneration` | `ManagementWorkspace.vue` |

## Tested

| عنصر | دليل |
|------|------|
| مفتاح تخزين الجلسة `auth` | `results.json` → `auth-storage-key` |
| Unit tests شكلية جديدة لهذه المرحلة | لم تُضف؛ اختبارات SalesRequests الحالية تبقى |

## Live-verified

| عنصر | دليل |
|------|------|
| `/management` → أول تبويب مسموح | admin + limited → `/management/branch/customers` |
| تبويبات مقفولة دون تركيب/طلبات بيانات | limited: locked=6 enabled=1 |
| رابط مباشر بلا صلاحية → AccessDenied بلا حلقة | `exceptionsListReqs=0` → `/access-denied?from=...` |
| إعادة استخدام صفحات (exceptions / followup) | `admin-exceptions.png`, `admin-followup.png` |
| إعادة تقييم بعد تحديث الجلسة على تبويب مسموح | `limited-refresh-allowed-tab` |
| توسعة السايدبار `aria-controls` فريد | `["nav-section-management-children"]` |
| لقطات 360 / 768 / 1440 حقيقية | `admin-360.png`, `admin-768.png`, `admin-1440.png`, `limited-*` |
| أيقونة البحث مع الخلفية الحالية | `admin-search-icon.png` |
| تبديل الفرع: gen↑، clear، طلبات `branchId=2` فقط | `branch_switch.json` + `admin-after-branch-switch.png` |

## Blocked

| عنصر | السبب |
|------|------|
| نشر إنتاج / لقطات CDN حية | خارج نطاق الإغلاق المحلي المطلوب |
| نقرة UI على مبدّل الفرع من السايدبار | فشل ترميز اسم الفرع في اللقطة؛ العزل Live-verified عبر `changeSelectedBranch` |
| Unit tests جديدة للمرحلة | لم تُطلب ضمن النطاق |

## مسارات التنقل

| مسار | الاسم | صلاحية |
|------|-------|--------|
| `/management` | ManagementMainRoute | anyOf صلاحيات التبويبات الجاهزة |
| `/management/branch/customers` | ManagementBranchCustomersRoute | Customer.Read |
| `/management/branch/payments` | ManagementBranchPaymentsRoute | OrderList.Read |
| `/management/branch/sales` | ManagementBranchSalesRoute | Order.Read |
| `/management/branch/sale-requests` | ManagementBranchSaleRequestsRoute | anyOf SalesRequest tabs |
| `/management/branch/sale-followup` | ManagementBranchSaleFollowupRoute | SalesFollowup.Read |
| `/management/branch/users` | ManagementBranchUsersRoute | User.Read |
| `/management/branch/activity-log` | ManagementBranchActivityLogRoute | ActivityLog.Read |
| `/management/delegate/exceptions` | ManagementDelegateExceptionsRoute | TabExceptions \| ExceptionDecide |
| `/management/delegate/sales-employee-rating` | ManagementDelegateSalesEmployeeRatingRoute | SalesFollowup.Read |
| `/management/delegate/accounts` | ManagementDelegateAccountsRoute | User.Read ∧ ManagePermissions |

## عقود موجودة مستخدمة (Existing source)

انظر `Management_Endpoints.xlsx` / `reference.json` — لا endpoints جديدة في هذه المرحلة.
الاستثناءات تستخدم عقود SalesRequests الحالية (`approve-exception` / `reject-exception`).
متابعة المبيعات تستخدم `SalesFollowup` الحالية.

## غير منفّذ (Not implemented)

المهام، الشكاوى، زيارات الفرع، القرارات القانونية، تقييم اللجان الشامل — موثّقة في السجل فقط (prompts 02–06).
