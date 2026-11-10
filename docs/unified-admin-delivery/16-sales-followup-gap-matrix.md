# 16 — مصفوفة فجوات متابعة المبيعات

مصدر: `director-sales-followup.vue` + APIs الحالية.

| الحقل/الزر | دليل المصدر | المقابل الحالي | الحالة | التصحيح | اختبار |
|---|---|---|---|---|---|
| قائمة الموظفين | موظف مبيعات بالمحافظة | كانت SellerId فقط | مختلف→مُصلَح | SalesEmployeeAssignment نشط | بلا تكليف لا يظهر |
| تفاصيل الأداء | KPI + زبائن + فئات | GetDetail batched | مطابق | احتفاظ | تفاصيل بعد تكليف |
| تقييم حقيقي/وهمي | حاسبة الزبون | CustomerRatingCalculator | مطابق | — | حدود الفئات |
| تقييم المدير | upsert score/note | UpsertManagerRating −10..10 | مطابق | بوابة تكليف | Read لا يقيّم |
| حظر التطبيق | block/unblock | ManageAppBlock | مطابق+idempotent | لا يكرر أثر نفس الحالة؛ يمنع login/shift فقط | حظر ثم login |
| تصدير Excel زبائن | exportToExcel | ناقص في اللوحة | ناقص موثّق | لا يُحجب القبول؛ دفعة لاحقة اختيارية | — |
| CollectionDelegateRatings | قديم | فارغ عمدًا | مختلف موثّق | خارج نطاق المبيعات | — |
| fan-out كل المحافظات | gateway | branchId موحّد | مختلف مقصود | نطاق الفرع | فرع ممنوع 403 |

## معادلات التقييم
- **حقيقي**: ScoreFor(..., usePhantomSaleDate: false)
- **وهمي**: ScoreFor(..., usePhantomSaleDate: true) — تعديل تاريخ البيع وفق AdjustSaleDate
- فئات الفرد: ممتاز/جيد/ضعيف/متلكئ/سيء من LevelName
- متوسط البطاقة: RatingAggregator / SalesFollowupRules.AverageRatingLabel
- إسناد البيع: Order.SellerId الأصلي — لا نقل عند إعادة التكليف
