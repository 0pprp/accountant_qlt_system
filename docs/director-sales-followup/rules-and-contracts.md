# قواعد وعقود متابعة المبيعات

المصدر: `DirectorSalesFollowupRules` / `DirectorSalesFollowupService` في SalesHaider.
النظام الحالي يستخدم مقياس تقييم `10/5/0/-5/-10` عبر `CustomerRatingCalculator` (وليس 0–10 القديم).

## Deploy marker

`SalesFollowup_V1_BranchScoped_BatchedDetail`

## مقياس «منخفض» على اللوحة

| قديم (0–10) | حالي (10/5/0/-5/-10) |
|-------------|----------------------|
| Low ≤ 6 | Low = درجة ≤ 0 (ضعيف / متلكئ / سيء) |
| High ≥ 7 | High = 5 أو 10 (جيد / ممتاز) |

حوار التقييمات المنخفضة: anonymize (درجة + ملاحظة + تاريخ + kind فقط — بلا اسم زبون).

## مصفّر (Zeroed)

```
!IsLegal && AmountTotalSales > 0 && ReceiptsTotal >= AmountTotalSales && AmountRemaining <= 0
```

`IsLegal` غير موجود في مخطط الزبون الحالي → يُعامل كـ `false` حتى يُضاف العمود (انظر SCHEMA_REQUIRED).

## تحويلات 30 يوم

مقارنة يومية لحالة التصنيف على زبائن الرصيد المفتوح (`AmountRemaining > 0`):
أهداف: ممتاز / جيد / ضعيف / متلكئ / سيء / قانونية.
ملخص: ازداد · نقص · مساوي.

## أداء النداءات

| حدث UI | طلبات HTTP |
|--------|------------|
| اختيار فرع | 1 × `GET employees?branchId=` |
| اختيار موظف | 1 × `GET employees/{id}?branchId=` (يشمل الزبائن + KPIs) + اختياريًا 1 × app-block إن لم يُضمَّن |
| لا طلب داخل حلقات صفوف الجدول | — |

## صلاحيات

| Permission | الغرض |
|------------|--------|
| `SalesFollowup.Read` | تبويب المبيعات + قراءة |
| `SalesFollowup.ManageAppBlock` | إيقاف/تفعيل تطبيق الموظف |
| `SalesFollowup.UpsertManagerRating` | تقييم المدير |

## موضع الواجهة

Route: `/sale/requests?tab=sales` — تبويب **المبيعات** داخل مساحة طلبات البيع.
