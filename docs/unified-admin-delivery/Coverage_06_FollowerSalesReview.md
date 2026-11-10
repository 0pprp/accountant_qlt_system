# Coverage_06 — مصادقة البيع (عمل المتابعين)

تاريخ: 2026-11-08  
لا دفعة 07. لا إعادة بناء 05.

## الحكم

| بند | حالة |
|---|---|
| من يصادق | المتابع (Motaba) من التطبيق — مثبت |
| لوحة المدير/المراقب | قراءة فقط — بلا زر اعتماد |
| سجل المصادقة | `FollowerSaleAuthentications` + migration |
| قراءة اللوحة | GET sales-review + remaining-customers |
| كتابة التطبيق | POST authenticate + حقول التقييم/الملاحظة/البضاعة/الوارد |
| أقسام 07–13 | not_implemented |

## العقود

| Endpoint | صلاحية |
|---|---|
| GET `/admin/FollowerWork/{id}/sales-review` | FollowerSalesReview.Read |
| GET `/admin/FollowerWork/{id}/remaining-customers` | FollowerSalesReview.Read |
| POST `/app/committee/pending-sales/{orderId}/authenticate` | FollowerSalesReview.Authenticate |

## تكييفات

- وحدة السجل = `OrderId` (بدل SaleDraftId).
- حقول تقييم المندوب/الموظف غير موجودة بالجديد → null.
- توجيه مبيع خاطئ ≤6: **أُغلق في الدفعة 31** عبر `WrongSaleNotice` عند المصادقة (انظر Coverage_31) — بلا أثر مالي مخترع.

## اختبارات

- وحدة قواعد المصادقة/الـbuckets
- تنقل: followerSales ready؛ declining not_implemented
- Live UI/screenshots: معلّق لجلسة معزولة بعد تطبيق migration
