# خريطة التبويبات — مصدرية فقط

تاريخ التدقيق: 2026-11-08  
المرجع: `C:\GitHub\SalesHaider-main\SalesHaider-main`  
الجديد: `C:\GitHub\accountant_qlt_system`  
**وضع المراجعة:** مصدرية فقط — لم يُشغَّل `FE_Company` ولا تطبيقات Flutter أثناء التدقيق؛ لا ادّعاء تطابق بصري.

انظر أيضاً: `GitSnapshot_BeforeAudit.md` (التغييرات الحالية محفوظة؛ لا reset).

---

## 0) أين تقع المجالات الثلاثة ذات الأولوية؟

| المجال | أين يوجد فعلياً في المرجع | هل في BM Flutter؟ | هل في DM Flutter؟ |
|---|---|---|---|
| **لوحة متابعة المندوبين** | `FE_Company` → `/director-delegate-followup` (المدير + المراقب) | لا | لا |
| **عمل المتابعين** | `FE_Company` → `/director-follower-work` + تبويبات فرعية (المدير + المراقب) | لا | لا (شاشة committees يتيمة غير مربوطة) |
| **الملخص** | `FE_Company` → `/director-summary` (**المدير فقط**) | لا | لا (الرئيسية ≠ الملخص) |

**تحذير ملزم:** تشابه الاسم «متابعة المبيعات» / «تقييم» / «مهام» في اللوحة الجديدة **ليس** دليلاً على تكافؤ وظيفي مع المجالات أعلاه.

---

## 1) مدير الفرع — `branch_manager_application`

التنقل: `Drawer` + `IndexedStack` في `lib/ui/home_shell.dart` — كل التبويبات ظاهرة بلا شرط صلاحية في الواجهة.

| # | التسمية | الشاشة | ملف المصدر | API رئيسي |
|---|---|---|---|---|
| 0 | مهامي | AssignedTasksPage | `ui/assigned_tasks_page.dart` | `branch-manager/assigned-tasks/inbox` |
| 1 | أسماء قانونية | DecisionsPage | `ui/decisions_page.dart` | CustomerDecisions / waivers |
| 2 | الزبائن المتدنية | DecliningCustomersPage | `ui/declining_customers_page.dart` | `branch-manager/customers/declining` |
| 3 | الزبائن | CustomersPage | `ui/customers_page.dart` | Customers_GetAll |
| 4 | المبيعات | NewSalesPage | `ui/new_sales_page.dart` | `branch-manager/new-sales` |
| 5 | المبيعات الخاطئة | WrongSalesPage | `ui/wrong_sales_page.dart` | `branch-manager/wrong-sales` |
| 6 | التقييم | StatsPage | `ui/stats_page.dart` | `branch-manager/stats/*` |
| 7 | التسديدات | PaymentsPage | `ui/payments_page.dart` | CustomersPayments_GetAll |
| 8 | طلبات التنازل | WaiversHistoryPage | `ui/waivers_history_page.dart` | `branch-manager/waivers` |
| 9 | سجل القرارات | DecisionsHistoryPage | `ui/history_page.dart` | CustomerDecisions/Decisions |

بوابة قبل المنزل: Login → ProvincePicker → BranchVisit استمارة دخول.  
مغادرة الفرع: ورقة تقييم قوائم المندوبين 0–10 ثم depart.

---

## 2) المدير المفوض — `delegated_manager_application`

التنقل: `HomeShell` + `HomeShellTabs` (`features/shell/presentation/home_shell.dart`).

| # | التسمية | الشاشة | ملف المصدر |
|---|---|---|---|
| 0 | الرئيسية | DashboardScreen | `features/dashboard/...` |
| 1 | مهامي | AssignedTasksInboxScreen | `features/assigned_tasks/...` |
| 2 | التقييمات | StatsPage | `features/stats/...` |
| 3 | الرسائل والشكاوى | ComplaintsScreen | `features/complaints/...` |
| 4 | طلبات الاستثناء | ExceptionsScreen(hold) | `features/exceptions/...` |
| 5 | المبيعات الخاطئة | ExceptionsScreen(wrongSale) | نفس الصنف |
| 6 | طلبات التنازل | WaiversScreen | `features/waivers/...` |
| 7 | حسابات إدارة الفرع | BranchManagerUsersScreen | `features/branch_manager_users/...` |
| 8 | أعمال إدارة الفرع | BranchVisitsScreen | `features/branch_visits/...` |
| 9 | الإعدادات | SettingsScreen | `features/settings/...` |

---

## 3) المدير / المراقب — `FE_Company` (مصدر المجالات الثلاثة)

قائمة رأسية: `FE_Company/src/navigation/vertical/index.js`  
حراسة المسارات: `plugins/1.router/index.js` (`directorAllowed` / `monitorAllowed`).

### ترتيب القائمة (المدير)

1. **الملخص** → `director-summary` (**المدير فقط**)
2. **متابعة المندوبين** → `director-delegate-followup` (مدير + مراقب)
3. **متابعة المبيعات** → مسار منفصل (ليس متابعة المندوبين)
4. مجموعة **عمل المتابعين** → `director-follower-work?tab=...` (مدير + مراقب):
   - مصادقة البيع (`sales`)
   - المتدنية (`declining`)
   - الملاحظات (`notes`)
   - الزبائن القديمة (`oldCustomers`)
   - ملاحظات المندوبين (`delegateNotes`)
   - الجباية (`collection`)
   - الموقع المباشر / المسارات (مسارات منفصلة)
5. عناصر أخرى للمدير (مهام اللجنة، تقييمات، …) — خارج أولوية هذا التسليم

المراقب: يدخل متابعة المندوبين وعمل المتابعين؛ **لا** يدخل الملخص.

---

## 4) اللوحة الجديدة — `Management` اليوم

مصدر السجل: `panel-admin-main/.../Management/domain/sections.ts`

### إدارة الفرع (ready)

| تبويب | مسار | المكوّن الفعلي | ملاحظة تكافؤ |
|---|---|---|---|
| العملاء | branch/customers | CustomerView | قريب من زبائن BM جزئياً |
| التسديدات | branch/payments | PaymentListPage | قريب من تسديدات BM جزئياً |
| المبيعات | branch/sales | SaleListPage | قريب من مبيعات BM جزئياً |
| طلبات البيع | branch/sale-requests | SalesRequestsWorkspace | ليس في BM القديم كهيكل واحد |
| **متابعة المبيعات** | branch/sale-followup | **SalesFollowupPage (SellerId)** | ≠ متابعة المندوبين |
| المستخدمون | branch/users | UserListPage | — |
| سجل النشاط | branch/activity-log | ActivityLogListPage | ≠ سجل القرارات القانونية |
| المهام المسندة | branch/assigned-tasks | ManagementTasksPage | يقابل «مهامي» إدارياً — ليس عمل المتابعين |

### المدير المفوض (ready)

| تبويب | مسار | المكوّن الفعلي | ملاحظة تكافؤ |
|---|---|---|---|
| الاستثناءات | delegate/exceptions | SalesRequestsWorkspace | يقابل طلبات الاستثناء جزئياً |
| **تقييم موظفي المبيعات** | delegate/sales-employee-rating | **نفس SalesFollowupPage** | تسمية مضلّلة؛ ليست متابعة مندوبين ولا تقييم لجان |
| إدارة الحسابات | delegate/accounts | UserListPage | يقابل حسابات إدارة الفرع جزئياً |
| المهام المسندة | delegate/assigned-tasks | ManagementTasksPage | يقابل «مهامي» DM — ليس عمل المتابعين |

**غير موجود في الجديد:** متابعة المندوبين، عمل المتابعين (بكل تبويباته)، الملخص المالي للمدير، أسماء قانونية، زبائن متدنية BM، مبيعات خاطئة BM/DM، تنازلات، زيارات فرع، شكاوى DM، تقييمات Stats بنطاق قائمة/متابع كما في المرجع.

---

## 5) خلاصة الربط الخاطئ (لا حذف — فك ربط فقط لاحقاً)

1. لا تستبدل **متابعة المندوبين** بـ SalesFollowup (موظفو مبيعات / SellerId).
2. لا تستبدل **عمل المتابعين** بـ ManagementTasks.
3. لا تستبدل **الملخص** بـ Dashboard العام في Core Home.
4. أبقِ SalesFollowup وManagementTasks كوظائف مستقلة إن بقيت مطلوبة — لكن انقل تسميات/أماكن التبويب لتطابق المرجع عند التنفيذ اللاحق.
