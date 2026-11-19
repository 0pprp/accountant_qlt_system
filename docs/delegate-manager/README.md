# المدير المفوض — المرجع

نقل تطبيق Flutter القديم `delegated_manager_application` (SalesHaider `origin/main`، الإصدار 1.0.19) إلى قسم «المدير المفوض» الموجود في لوحة الويب. الصفحة القديمة نفسها، وليست تبويباً جديداً.

- **التصميم المعتمد:** https://www.figma.com/design/KzwPm2C9XXObEjUu399jLt (اللوحة، الاستثناءات، التنازل، مهامي، الجوال).
- **المسار:** `/management/delegate/<القسم>?branchId=`.

## الملفات

| الملف | المحتوى |
|---|---|
| [rules.md](rules.md) | قاعدة كل قسم وإجراء مع مرجع المصدر ومقابله هنا |
| [differences.md](differences.md) | عيوب القديم، وما أُصلح، ولماذا |

## اللوحة

- **الرأس:** «المدير المفوض»، وفلتر «كل المحافظات / فرع» في `?branchId=`. الفلتر محفوظ في المتصفح، ويطبَّق على كل الأقسام. معه زر «تحديث».
- **العمود الجانبي:** الأقسام العشرة بترتيب التطبيق القديم، مع عدّادات من `GET /admin/DelegateHome/summary?branchId=`:

  | القسم | العدّاد |
  |---|---|
  | مهامي | المفتوحة عندي |
  | الشكاوى | غير المقروءة |
  | الاستثناءات | المعلقة |
  | الخاطئة | غير المقروءة |
  | التنازل | المعلق |

  على الجوال يصير العمود قائمة منسدلة.
- **المحتوى:** كل قسم صفحة داخل بطاقة اللوحة. الصفحات تقرأ `branchId` من الرابط.

## أين أعدّل؟

| أريد تغيير… | الملف |
|---|---|
| غلاف اللوحة والعدّادات والفلتر | `panel-admin-main/panel-admin-main/src/modules/DelegateManager/components/DelegateManagerBoard.vue` |
| من يعرض اللوحة | `src/modules/Management/views/ManagementWorkspace.vue` (عند `appSection === 'delegate'`) |
| ترتيب الأقسام وأسماؤها | `src/modules/Management/domain/workspaceNavigation.ts` (`APP_TABS.delegate`) |
| ملخص الرئيسية | `src/modules/DelegateHome/views/DelegateHomePage.vue`، `api-main/api-main/src/Application/DelegateHome/Queries/GetDelegateHomeSummaryQuery.cs` |
| طلبات الاستثناء | `src/modules/DelegateManager/views/DelegateExceptionsPage.vue`، `Application/SalesRequests/Queries/Admin/ExceptionInbox/GetExceptionInboxQuery.cs`، `Commands/Admin/MarkExceptionRead`، `Common/SalesRequestExceptionDecider.cs` |
| الرسائل والشكاوى | `src/modules/Complaints/views/ComplaintsPage.vue`، `Application/Complaints/**` |
| المبيعات الخاطئة | `src/modules/WrongSales/views/WrongSalesPage.vue` (`audience="delegate"`)، `Application/WrongSales/**` |
| طلبات التنازل | `src/modules/Waivers/views/WaiversPage.vue` (`scope="delegate"`)، `Application/Waivers/**` |
| مهامي | `src/modules/ManagementTasks/**` (`inboxOnly`)، `Application/ManagementTasks/**` |
| التقييمات | `src/modules/ManagementRatings/**` |
| حسابات إدارة الفرع | `src/modules/BranchManagerAccounts/**` |
| أعمال إدارة الفرع | `src/modules/BranchVisits/views/BranchVisitsPage.vue` (`mode="history"`) |
| الإعدادات | `src/modules/DelegateSettings/views/DelegateSettingsPage.vue` |

## قاعدة البيانات

- **migration واحدة:** `20261118204124_SalesRequestExceptionReadAt`.
  - تضيف عمود `SalesRequests.ExceptionReadAtUtc` (nullable)، ويعيد `Down` الحالة السابقة.
  - العمود يُصفّر عند طلب استثناء جديد (`HoldForException`).
  - يُملأ عند فتح الطلب أو عند القرار.
