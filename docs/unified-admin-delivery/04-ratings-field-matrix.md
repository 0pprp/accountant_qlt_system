# 04 — مصفوفة حقول التقييمات العامة

إثبات: مطابقة مصدرية من `director-ratings.vue` + `BranchManagerStatsService` / `BranchMonthStatsService` / `CustomerRatingCalculator`.  
ليست SalesFollowup ولا تقييم عامل اللوحة DelegateFollowup. لا دفعة 05.

## التنقل والفلاتر

| عنصر | مصدر | سلوك | جديد |
|---|---|---|---|
| مركز المحافظات | hub بدون `?city=` | بطاقات فروع مسموحة + ملخص شهر | `/management/ratings` |
| تفاصيل محافظة | `?city=` | فلاتر scope + فترة بيع + رسم + فئات | `?branchId=` validated |
| scope | branch / follower / employee | قائمة = drill من كروت المتابع | allowlist نفسها |
| فترة | fromDate/toDate افتراضي أول الشهر→اليوم | **تاريخ البيع فقط**؛ التقييم حتى اليوم | Asia/Baghdad |
| رجوع من بروفايل | session return state | يستعيد branch/scope/entity/period/bucket | URL + session خفيف |
| تصدير | غير موجود في المرجع | — | لا Export |

## المقاييس والبطاقات

| الاسم | مصدر | معادلة | فترة | داخل | مستثنى | جديد | صلاحية | اختبار |
|---|---|---|---|---|---|---|---|---|
| تقييم الفرع (عنوان) | ActiveRatingCohort + ListRatingAggregator | متوسط نقاط زبائن نشطين (آخر تسديد ≤30ي + Remaining>0) | asOf اليوم | نشط مفتوح | مصفّر؛ بلا تسديد 30ي | نفس `CustomerRatingCalculator.ScoreFor` | Read | عينة فئات |
| تقييم الفترة | PeriodAggregate | متوسط لمن باعوا ضمن [from,to] مقيَّمين حتى اليوم | نافذة البيع | DateSale في النافذة | بلا تاريخ بيع | RatingTrendCalculator | Read | حدود الشهر |
| التقييم الوهمي | PhantomSaleDate | نفس المجموعة مع AdjustSaleDate | asOf | نفس النشط | — | `usePhantomSaleDate:true` | Read | اختلاف عن الحقيقي |
| فئات ممتاز…سيء | Aggregate counts | عتبات متوسط القائمة ≥7/≥3/≥0/>−7.5 | asOf | نقاط فردية 10/5/0/−5/−10 | — | RatingAggregator | Read+Details | حد شامل |
| زبائن الفئة | GetRatingCustomers | نفس ActiveRatingCohort (list/follower/branch) أو نافذة بيع (employee) | asOf | المستوى المطلوب | مصفّر | customers API | DetailsRead | العدد=التفاصيل |
| اتجاه الرسم | نقاط يومية | Δ(end−start) على نقاط الفترة؛ عتبة 0.05 | chartFrom→اليوم | — | — | Direction up/down/flat | Read | نهاية/بداية |
| كروت hub شهر | BranchMonthStats | New=Remaining>0 وبيع في الشهر؛ Zeroed=Remaining=0 وآخر تسديد في الشهر؛ Net=New−Zeroed | شهر بغداد الحالي/السابق | فرع مسموح | — | period-summary | Read | يدوي |
| مجموع القسط للقائمة | Sum AmountDaySales | Remaining>0 على القائمة | asOf | — | — | installmentSum في trend | Read | |
| تسديدات قائمة | list-payments-sum | Σ InstallmentPayment.Date في [from,to] | inclusive | طلبات معتمدة | — | list-payments-sum | Read | يوم أمس افتراضي |

## تعارضات موثّقة

1. **نشاط القائمة للكتالوج = 40 يوم** (`DelegateFollowup` / LoadActiveLists) بينما **نشاط زبون التقييم = 30 يوم** (`ActiveCustomerRules`). لا توحيد صامت.
2. بعد بناء الرسم، `EndValue` يُستبدل بمتوسط المجموعة النشطة (PeriodIgnored=true) — اتجاه الرسم يبقى من نقاط نافذة البيع.
3. `IsLegal` لا يغيّر النقاط؛ يُخفى الواصل/الباقي في الاستجابة عند القانونية (إسقاط حقلي).
4. Employee في القديم يعتمد SalesDrafts/Requests؛ الجديد: `Order.SellerId` + Approved — موثّق كتكييف كيان.

## غير محسوم
- معادلة RatingEquationFactor الافتراضية غير المستخدمة في الجديد (عامل=1).
- لقطات تفاعلية حية معلّقة حتى جلسة معزولة.
