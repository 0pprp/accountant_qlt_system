# الأمان

## طبقات التحقق

1. **`[HasPermission]` على كل endpoint جديد:**

   | الصلاحية | الـ endpoint |
   |---|---|
   | `SalesRequest.ReturnRequest` | return |
   | `ManagerPrepare` | prepare |
   | `ManagerPend` | pend |
   | `CancelToUnassigned` | cancel-to-unassigned |
   | `TransferBranch` | transfer-branch |
   | `Assign` | assign-batch |
   | `DeleteSoldSale` | delete-sold و delete-sold-impact |
   | `DeletedSalesRead` | deleted-sales |

2. **فحص الفرع داخل كل handler:** `SalesRequestAccess.AuthorizedBranches`. وفي النقل يجب أن يكون الفرع الهدف ضمن نطاق المستخدم أيضاً.
3. **التزامن:** `SalesRequestConcurrency.SaveWithConflictGuardAsync` يرد 409 عند تعديل متزامن.
4. **الحالة:** كل إجراء يتحقق من الحالة في الباكند، والأزرار في الواجهة تطابقه ولا تغني عنه.

## الحِزم في محرر صلاحيات التبويبات (`AdminPanelTabs.cs`)

| التبويب | يضيف |
|---|---|
| `sr.unassigned` | TransferBranch |
| `sr.incoming` | CancelToUnassigned، TransferBranch |
| `sr.prepared` | ManagerPend، CancelToUnassigned |
| `sr.inspected` | ManagerPrepare، ManagerPend، CancelToUnassigned |
| `sr.pending` | ManagerPrepare، CancelToUnassigned |
| `sr.rejected` | ReturnRequest |
| `sr.legal` | CancelToUnassigned |

## حذف المبيع نهائياً

- **الصلاحية:** `DeleteSoldSale` و`DeletedSalesRead` داخل `SalesRequest.OutsideAll`، وتُمنح للأدمن تلقائياً. ليست في أي حزمة تبويب ولا في الأدوار الافتراضية.
- **الواجهة:** الزر بلون الخطر وفي آخر الصف، ولا يظهر لغير المصرّح.
- **قبل التنفيذ:** معاينة إلزامية للأثر، وسبب من 5 أحرف على الأقل، وتأكيد صريح إذا وُجدت تسديدات.
- **التنفيذ:** معاملة واحدة، فإذا فشلت أي خطوة لا يتغير شيء.
- **بعد الحذف:**
  - لا يُسترجع؛ `Restore` يرد `SoldSaleWiped`.
  - يختفي من «المحذوفة» للجميع.
  - يظهر للأدمن في «حذوفات المبيعات» (تبويب `managementDeletedSales`) مع الملخص.

## الاستثناء

- **الطلب:** `RequestException` يطلب.
- **القرار:** `ExceptionDecide` يقرر، ولا يقرر الطالب على طلبه (`ExceptionSelfApproval`).
- **الرفض:** ينقل الطلب إلى «مرفوض»، فلا يمكن إسناده بعدها لتجاوز الرفض.
