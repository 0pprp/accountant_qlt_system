# 04A — Transition Matrix (SalesRequest lifecycle)

مرجع: handlers/service القديم + كيان الجديد. أسماء التبويبات ليست قواعد انتقال.

## قرار ConvertedToSale

| | القرار |
|--|--------|
| المعنى في القديم | حالة طلب مرتبط بمسودة بيع (`ConvertedToSaleId`) — **ليست** «تم البيع» |
| في enum الجديد قبل 04A | **غائب** |
| قرار 04A | إضافة `ConvertedToSale = 11` + حقل `LinkedOrderId` صريح. الحالة تعني مسودة Order مربوطة؛ `Completed` فقط بعد إكمال بيع ناجح |
| لا | لا إسقاط المعنى؛ لا إعادة ربط Orders قديمة بالاسم/الهاتف |

## تبويبات العرض ≠ حالات متنافسة

| تبويب عرض | حالات SalesRequest |
|-----------|-------------------|
| غير المسند | `IsUnassigned()` |
| طلبات البيع | Assigned, Viewed, Returned |
| جاهز للبيع | PreparedForSale, InProgress, ConvertedToSale |
| تم الكشف | Inspected |
| معلقة | Pending |
| مرفوض | Rejected |
| تم البيع | Completed |

**تنبيه:** تبويب Flutter القديم `pending` = Order.WorkflowStatus — **ليس** `SalesRequest.Pending`.

## المصفوفة

| From | Action | To | Actor | Permission (مقترح) | Scope | Required data | Order effect | Timeline event |
|------|--------|-----|-------|-------------------|-------|---------------|--------------|----------------|
| New / unassigned | Assign | Assigned | Admin/SM | Assign | branch | eligible employeeId | none | Assigned |
| Assigned | View (first) | Viewed | Employee owner | AppView | ownership | — | none | Viewed |
| Viewed+ | View (repeat) | same | Employee | AppView | ownership | — | none | ViewSeen (no status bump) |
| Assigned/Viewed/Returned/Prepared/… | Reassign | Assigned | Admin | Assign | branch | new employeeId + reason | none; prior owner blocked | Reassigned |
| owned convertible* | Prepare | PreparedForSale | Emp | AppPrepare | ownership | note (emp) | none | Prepared |
| owned non-Completed | Pend | Pending | Emp | AppPend | ownership | note | none | Pend |
| owned non-Completed | Reject | Rejected | Emp | AppReject | ownership | reason | none | Rejected |
| owned | Inspect | Inspected | Emp | AppInspect | ownership | optional linkedOrderId | none unless already linked | Inspected |
| owned + no LinkedOrderId | LinkDraft (idempotent) | ConvertedToSale | Emp/App | AppLinkDraft | ownership | create/attach Order once | set LinkedOrderId unique | DraftLinked |
| LinkedOrderId + sale complete rules | CompleteSale | Completed | Emp | via CompleteSalesWorkflow | ownership + financials | existing sale completion | Order → Sold | Completed |
| non-Rejected (mgr) | Return | Returned | Admin | Assign/Return | branch | note; restore assignee | none | Returned |
| with match need | RequestException | hold Pending | Admin | RequestException | branch | reason | none | ExceptionRequested |

\*convertible: ليس Completed/Rejected/LegalFollowup/Deleted.

## قواعد ثابتة

1. الإسناد ذري: TargetEmployeeId + Status + ConcurrencyStamp + Timeline في معاملة واحدة — **لا** إنشاء Customer/Order.
2. `requestId` ≠ `orderId` في كل DTO.
3. قيد فريد على `LinkedOrderId` يمنع بيعًا مكررًا لنفس الطلب.
4. تعارض التزامن → 409 `SalesRequest_Concurrency_Conflict`.
5. الموظف السابق بعد إعادة الإسناد: أي كتابة → NotOwner / 409.
6. الطلبات القديمة بلا رابط Order: مسار توافق — تظهر في تبويب الحالة؛ البيع يبدأ بـ LinkDraft لاحقًا.

## عقود App النهائية (04A)

| Method | Path | Auth |
|--------|------|------|
| GET | `/api/v1/app/SalesRequests?tab=` | AppRead (+ session user) |
| GET | `/api/v1/app/SalesRequests/{requestId}` | AppRead + ownership/branch |
| POST | `/api/v1/app/SalesRequests/{requestId}/view` | AppView |
| POST | `.../prepare` | AppPrepare |
| POST | `.../pend` | AppPend |
| POST | `.../reject` | AppReject |
| POST | `.../inspect` | AppInspect |
| POST | `.../link-draft` | AppLinkDraft |

الإسناد الإداري يبقى `POST /api/v1/admin/SalesRequests/{id}/assign`.
