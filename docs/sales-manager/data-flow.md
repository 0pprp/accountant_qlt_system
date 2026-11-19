# تدفق البيانات

## من يكتب الطلب

| الكاتب | كيف | الحالة الناتجة |
|---|---|---|
| موظف المبيعات (التطبيق) | تقديم طلب | New (`EmployeeSubmitted`) |
| المتابع / المندوب (التطبيقات) | تقديم طلب | New (`Follower` / `Delegate`) |
| مدير المبيعات (اللوحة) | إسناد، إرجاع، تجهيز، تعليق، إلغاء، نقل، رفض، طلب استثناء | حسب rules.md |
| المدير المفوض | قرار الاستثناء | Approved أو Rejected |
| موظف المبيعات (التطبيق) | اطلاع، تجهيز، تعليق، كشف، رفض، ربط مسودة، إتمام البيع | Viewed … Completed |
| الأدمن | حذف المبيع نهائياً | محذوف + سجل نشاط |

## الجداول

- **`SalesRequests`:** الطلب وحالته، والموظف، والحجز، وبيانات الحذف.
- **`SalesRequestTimelineEntries`:** كل انتقال، ومنها الجديدة: CancelledToUnassigned=20، BranchTransferred=21، SoldSaleDeleted=22.
- **`SalesRequestChangeOutbox`:** يُبث عبر SignalR ليتحدّث كل من يرى الطلب.
- **`LegalFollowupCases`:** القضية القانونية. تُلغى عند «إلغاء لغير المسند».
- **عند حذف المبيع:**
  - تُمسح: `Orders`، `OrderItems`، `InstallmentPayments`، `Purchases`، `ArchiveTransactions`.
  - تُعدَّل: `Products.RemainingCount`، `Users.UndeliveredCashAmount`، `SafeMovements` (Refund).
  - يُكتب: `ActivityLogs`.

## الـ endpoints

القاعدة: `api/v1/admin/SalesRequests`

| الـ endpoint | الغرض |
|---|---|
| `GET ?tab=` و`GET counts` | التبويبات والعدّادات (لا تداخل) |
| `POST assign-batch` | الإسناد الجماعي |
| `POST {id}/return` | الإرجاع |
| `POST {id}/prepare`، `POST {id}/pend` | التجهيز والتعليق |
| `POST {id}/cancel-to-unassigned` | الإلغاء لغير المسند |
| `POST {id}/transfer-branch` | النقل لفرع آخر |
| `GET {id}/delete-sold-impact`، `POST {id}/delete-sold` | معاينة حذف المبيع وتنفيذه |
| `GET deleted-sales` | حذوفات المبيعات (للأدمن) |

## اللوحة

- **الاستعلامات:** TanStack Query بالمفاتيح الموجودة لطلبات البيع. كل إجراء يُبطل القائمة والعدّادات والطلب وسجله (`useSalesManagerActionMutation`).
- **الحالة في الرابط:** القسم في المسار، و`branchId` و`employeeId` في الـ query. الصفحات المضمّنة تحافظ على مسار اللوحة عند تغيير فلاترها.
