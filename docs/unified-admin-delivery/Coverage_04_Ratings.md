# Coverage_04 — التقييمات العامة

تاريخ: 2026-11-08  
إثبات: مطابقة مصدرية. لا استدعاء API القديم. لا دفعة 05.

## الهوية

| | قديم | جديد |
|---|---|---|
| مسار | `/director-ratings` | `/management/ratings` |
| صلاحية | DirectorOrMonitor | `ManagementRatings.Read` + `DetailsRead` |
| ليس | — | SalesFollowup / BoardOperator / ملخص |

## العقود

| Endpoint | صلاحية |
|---|---|
| GET branches | Read |
| GET catalog | Read |
| GET period-summary | Read |
| GET trend | Read |
| GET customers | Read **و** DetailsRead |
| GET list-payments-sum | Read |

## الحسابات المشتركة

| وحدة | موضع | مستهلك |
|---|---|---|
| CustomerRatingCalculator | SalesRatings | DF + Ratings |
| ActiveCustomerRules (30ي) | SalesRatings | Ratings headline |
| RatingAggregator | SalesRatings | Ratings (+ مطابق ClassifyAverageLabel في DF) |
| RatingTrendCalculator | SalesRatings | Ratings trend |
| نشاط قائمة 40ي | DelegateFollowupRules | catalog lists فقط |

## تكييفات موثّقة

- Employee: `Order.SellerId` بدل SalesDrafts.
- لا Export في المرجع → لا صلاحية Export.
- إسقاط AmountPaid/Remaining للقانونية في response.

## حالة الإغلاق

| بند | حالة |
|---|---|
| مصفوفة الحقول | منفّذ |
| API + صلاحيات + قوالب Director/Monitor | منفّذ |
| واجهة hub→تفاصيل→فئة | منفّذ |
| اختبارات وحدة حسابية | منفّذ |
| لقطات 360/768/1440 | معلّق |
| دفعة 05 | لم تبدأ |
