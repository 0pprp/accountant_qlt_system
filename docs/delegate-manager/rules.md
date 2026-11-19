# القواعد

- **مسارات القديم:** نسبية إلى `SalesHaider-main/SalesHaider-main` على `origin/main`.
  - **DM** = `delegated_manager_application/lib/features`
  - **GW** = `BE_SalesEmployee`
- **مسارات الجديد:** نسبية إلى `api-main/api-main/src/Application`.

## الرئيسية

| البطاقة | الحساب | القديم | الجديد |
|---|---|---|---|
| رسائل وشكاوى غير مقروءة | `ComplaintMessages.Status = Unread` في النطاق | GW `DelegatedManagerController.cs:254-281` | `DelegateHome/Queries/GetDelegateHomeSummaryQuery.cs` |
| طلبات استثناء قيد الانتظار | `ExceptionHoldStatus = Pending` | نفسه | نفسه |
| مبيعات خاطئة غير مقروءة | `DelegateReadAtUtc = null` | نفسه | نفسه |
| طلبات تنازل قيد الانتظار | `Status = Pending` | لم يكن يعرض | نفسه (جديد) |

- **الملخصان:**
  - **الاستثناءات:** موافق، مرفوض، ملغى، الإجمالي.
  - **التنازلات:** بانتظار المحاسب، مطبّق، مرفوض، الإجمالي.
- **عدّاد «مهامي»:** المهام التي أحملها بحالة مرسلة أو مقبولة أو بانتظار تصديقي.
- **النطاق:** فروع المستخدم، أو الفرع المختار من الأعلى إن كان ضمنها.

## طلبات الاستثناء

| البند | القاعدة | القديم | الجديد |
|---|---|---|---|
| الشرائح | قيد الانتظار، موافق عليه، مرفوض، ملغى، مع العدد | DM `exceptions/exceptions_screen.dart` | `SalesRequests/Queries/Admin/ExceptionInbox/GetExceptionInboxQuery.cs` |
| «ملغى» | طلب كان فيه استثناء، ثم أُلغي لغير المسند بعد طلب الاستثناء | GW `SalesExceptionService.cs` | تُشتق من سجل الأحداث (`CancelledToUnassigned` بعد `ExceptionRequested`) |
| الترتيب | الأحدث طلباً أولاً، 20 في الصفحة | GW `PagedResult.cs:22-36` | نفسه |
| البحث | اسم الزبون، الهاتف، الفرع | DM (على المحمّل فقط) | في الخادم |
| مقروء | فتح الطلب يعلّمه مقروءاً للجميع؛ القرار يعلّمه أيضاً | GW `POST exceptions/{id}/read` | `POST admin/SalesRequests/{id}/exception-read` |
| التفاصيل | التصنيف (زبون موجود/جديد)، الطلب الحالي، المصدر، السبب، بروفايل الزبون (سعر البيع، المدفوع، المتبقي، السندات، التسديدات، المبيعات، الحسابات، التقييمات)، مطابقات الفرع | DM `exception_detail_screen.dart` | `DelegateManager/views/DelegateExceptionsPage.vue` |
| القرار | موافقة أو رفض مع تأكيد؛ الملاحظة اختيارية ≤ 500 | DM نفسه | `Common/SalesRequestExceptionDecider.cs` |
| طالب الاستثناء | لا يقرر في طلبه | — | `SalesRequestExtraViewRules.CheckDecision` |
| الرفض | ينقل الطلب إلى «مرفوض» | GW | `SalesRequestExceptionDecider` |
| التعارض | قرار من مستخدم آخر → 409 «تم اتخاذ القرار من مستخدم آخر» ثم إعادة تحميل | DM `api_client.dart:75-96` | `SalesRequestConcurrency` |

## الصلاحيات

| الإجراء | الصلاحية |
|---|---|
| قراءة الصندوق والتعليم كمقروء | `SalesRequest.TabExceptions` أو `SalesRequest.ExceptionDecide` |
| القرار | `SalesRequest.ExceptionDecide` |
| الرئيسية | `DelegateHome.Read`، وكل بطاقة بصلاحية قسمها |
| الشكاوى | `Complaint.Read`، والتعليم `Complaint.MarkRead` |
