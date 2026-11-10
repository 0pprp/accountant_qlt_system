# 06 — مصفوفة حقول مصادقة البيع

## salesAuth (صف المبيع)

| العرض | المصدر القديم | المعنى | الجديد | الصلاحية | قبول |
|---|---|---|---|---|---|
| customerId / name / phone | Customers | هوية الزبون | Customer عبر Order | Read | يظهر |
| listId / listName | Delegates / List | قائمة التكليف | OrderList | Read | يظهر |
| orderId | SaleDraftId / CustomerSaleId | وحدة المبيع | Order.Id | Read | يظهر |
| authenticated | وجود FollowerSaleAuthentications | هل سُجّلت مصادقة | وجود FollowerSaleAuthentication | Read | bool |
| authenticatedAtUtc | AuthenticatedAtUtc | وقت المصادقة | AuthenticatedAtUtc | Read | ISO |
| authRating / authNote | Rating / Note | تقييم/ملاحظة المتابع | Rating / Note | Read | 1–10 |
| shopStock / dailyRevenue | من استمارة المتابع | بضاعة/وارد | ShopStockEstimatedValue / EstimatedDailyRevenue | Read | &gt;0 عند الكتابة |
| shopRating* (مندوب) | DelegateFirst / SalesRequests | تقييم محل مندوب | — | — | null (فجوة) |
| employee shop* | SalesShopProfiles | موجودات موظف | — | — | null (فجوة) |
| salePrice / saleDate / dailyInstallment | Draft / View | بيانات البيع | Order.SellAmount / SaleDate / DailyInstallment | Read | |
| sellerName | User | البائع | Order.Seller.FullName | Read | |
| paid / remaining | View_Customers | واصل/متبقي | مجموع دفعات / المتبقي | Read | زبائن نشطون للحزم |
| currentRatingLabel | phantom | تقييم النظام الحالي | ActiveRatingCohort label | Read | |
| خطر منخفض | auth/shop ≤6 | شارة خطر | authRating ≤6 فقط (بلا shop إن غاب) | Read | نص+لون |

وحدة السجل: **طلب/مبيع واحد**. الزبون قد يظهر أكثر من مرة إن له أكثر من طلب بانتظار/مصادق.

## workBundle (ضمن قسم المبيعات فقط)

| مجموعة | المعنى | وحدة العد | فترة | الجديد |
|---|---|---|---|---|
| listCards | تقييم وهمي لكل قائمة مسندة | قائمة | asOf بغداد + أسبوع | قوائم Motaba + SalesRatings |
| ratingBuckets | ممتاز/جيد/ضعيف/متلكئ/سيء | زبائن مقيّمون نشطون (رصيد مفتوح + نشط 30ي) | اليوم vs أمس | ActiveRatingCohort |
| remainingLe100 | باقي &gt;0 و&lt;100000 | زبائن نشطون | اليوم vs أمس | نفس الحقائق |
| remainingLt200 | باقي ∈[100000,200000) | زبائن نشطون | اليوم vs أمس | نفس الحقائق |

لا تُعاد بيانات ملاحظات/جباية/مسارات داخل هذا الـbundle.

## remaining-customers buckets (allowlist)

`excellent|good|weak|lagging|bad|le100|lt200|rated`

| bucket | فلتر |
|---|---|
| excellent…bad | مستوى التقييم الوهمي |
| le100 | 0 &lt; remaining &lt; 100000 |
| lt200 | 100000 ≤ remaining &lt; 200000 |
| rated | كل النشطين المقيّمين |

## أزرار اللوحة

| زر | مصدر | حالة 06 |
|---|---|---|
| فتح bucket | openRemainingBucket | منفّذ قراءة |
| بروفايل مختصر + عودة | profileRow / returnTo | منفّذ |
| بروفايل كامل | sales-manager-customer-profile | رابط AuditProfile مع return آمن |
| مصادقة من اللوحة | — | **غير موجود** |

## عقود API

| Method | Path | Auth |
|---|---|---|
| GET | `/admin/FollowerWork/{followerId}/sales-review?branchId=` | FollowerSalesReview.Read |
| GET | `/admin/FollowerWork/{followerId}/remaining-customers?branchId=&bucket=` | FollowerSalesReview.Read |
| POST | `/app/committee/pending-sales/{orderId}/authenticate` | FollowerSalesReview.Authenticate |