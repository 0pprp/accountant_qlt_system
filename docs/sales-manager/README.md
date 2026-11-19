# إدارة المبيعات — المرجع

نقل صفحة مدير المبيعات من SalesHaider (`origin/main`، تبويب «إدارة المبيعات» بـ7 صفحات) إلى تبويب «المبيعات» الموجود في النظام الجديد. لا يوجد تبويب جديد: صفحات «المبيعات» نفسها صارت لوحة واحدة.

## الملفات

| الملف | المحتوى |
|---|---|
| [rules.md](rules.md) | التبويبات وانتقالات الحالة وقاعدة كل إجراء، مع مرجع المصدر ومقابله هنا |
| [data-flow.md](data-flow.md) | من يكتب الطلب ← الجداول ← الـ endpoints ← اللوحة |
| [security.md](security.md) | الصلاحيات، عزل الفروع، الحذف النهائي |
| [differences.md](differences.md) | عيوب القديم وما أُصلح ولماذا |

## اللوحة

المسارات نفسها الموجودة تحت `/sale/...` (`/sale/overview`، `/sale/requests`، `/sale/employees`، `/sale/followup`، `/sale/live-map`، `/sale/routes`، `/sale/completed`، `/sale/summary`، `/sale/legal`، `/sale/excel-search`)، مع `?branchId=&employeeId=`.

- `SalesManager/router.ts` يستبدل مكوّن كل مسار من هذه بمكوّن اللوحة، ويبقي اسمه ومساره وصلاحيته (`accessRequirement`) كما هي.

- **العمود الجانبي:**
  - «كل الطلبات» في أعلاه، ثم موظفو المبيعات.
  - لكل موظف: التقييم الحقيقي، والدوام، ونقطة لحالة الموقع، وعدد مبيعات اليوم والمعلّق.
  - على الجوال يصير قائمة منسدلة.
- **التبويبات:** نظرة عامة، طلبات البيع، موظفو المبيعات (التقييم)، متابعة المبيعات، الموقع المباشر، المسارات، المبيعات المنجزة، ملخص المبيعات، المتابعة القانونية، بحث Excel.
  - يظهر كل تبويب لمن يملك صلاحيته فقط.
  - كل تبويب يعيد استخدام صفحته الموجودة، مضمّنة داخل اللوحة.
- **اختيار موظف:** يفلتر الطلبات (`employeeId`) والمبيعات (`sellerId`) والمسارات. «موظفو المبيعات» و«المسارات» تحتاجان موظفاً محدداً.
- **السايدبار:** مجموعة «المبيعات» الموجودة بلا تغيير، وكل عنصر فيها يفتح اللوحة على قسمه.

## أين أعدّل؟

| أريد تغيير… | الملف |
|---|---|
| أي طلب في أي تبويب | `api-main/api-main/src/Application/SalesRequests/Common/SalesRequestAdminQuery.cs` (`TabQuery`، `TabKeyFor`)، `SalesRequestAccess.UnassignedInScope`، `Domain/Services/SalesRequestLifecycle/SalesRequestTabMap.cs` |
| الإرجاع | `Application/SalesRequests/Commands/Admin/Return/*` |
| التجهيز / التعليق | `Commands/Admin/ManagerProgress/*` (`ManagerProgressRules`) |
| الإلغاء لغير المسند | `Commands/Admin/CancelToUnassigned/*`، `SalesRequest.CanCancelToUnassigned` |
| النقل لفرع آخر | `Commands/Admin/TransferBranch/*`، `SalesRequest.CanMoveToBranch` |
| الإسناد الجماعي | `Commands/Admin/AssignBatch/*` (يعيد استخدام `Assign`) |
| حذف المبيع نهائياً | `Commands/Admin/DeleteSoldSale/*` |
| حذوفات المبيعات (الأدمن) | `Queries/Admin/DeletedSales/GetDeletedSalesQuery.cs`، تبويب `managementDeletedSales` في `AdminPanelTabs.cs` |
| رفض الاستثناء | `Common/SalesRequestExceptionDecider.cs` |
| المطابقة | `Domain/Services/SalesMatching/*`، `SalesMatchCandidateSelector.cs` (عمل جلسة المطابقة) |
| الصلاحيات | `Application/Common/SeedData/Permissions.cs` (`SalesRequest.*`)، حِزم التبويبات في `AdminPanelTabs.cs` |
| اللوحة | `panel-admin-main/panel-admin-main/src/modules/SalesManager/views/SalesManagerBoard.vue` |
| أقسام اللوحة وصلاحياتها | `src/modules/SalesManager/domain/sections.ts`، `router.ts` |
| أزرار إجراءات الطلب | `src/modules/SalesRequests/components/RequestLifecycleActions.vue` |
| تعريف التبويبات في الواجهة | `src/modules/SalesRequests/domain/tabs.ts` |
| السايدبار | مجموعة «المبيعات» الموجودة في `src/modules/Core/navigation/navigation.registry.ts` (لم تتغير) |

## الاختبارات

- **`tests/Application.Tests/SalesRequests/SalesManagerActionsTests.cs`:**
  - الإرجاع والتجهيز والتعليق والإلغاء والنقل.
  - الإسناد الجماعي.
  - رفض الاستثناء ← «مرفوض».
- **`SalesRequestExtraViews04ETests.cs`:** أن كل طلب يقع في تبويب واحد (`Every_request_lands_in_exactly_one_tab`).
- **الواجهة:** `src/modules/SalesRequests/composables/__tests__/useSalesRequestTabAccess.test.ts`.
