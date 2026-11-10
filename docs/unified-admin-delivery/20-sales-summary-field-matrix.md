# 20 — مصفوفة ملخص المبيعات

مصدر: `FE_Company/src/pages/sales-manager-sales-summary.vue` → `directorSummaryMatrix` / `directorSummaryDrilldown` (metric ثابت `salePrice`).

| العنصر | المصدر | المعنى / الاحتساب | الفلاتر الافتراضية | المقابل الجديد | النقص |
|---|---|---|---|---|---|
| عنوان الشاشة | sales-manager-sales-summary | ملخص سعر البيع حسب المحافظة والأيام | — | `/sale/summary` | — |
| السنة/الشهر | year/month refs | شهر تقويمي بغداد عبر SaleDate | سنة بغداد الحالية + شهرها | year/month query | — |
| المقياس | `metric = 'salePrice'` ثابت | مجموع SellAmount لبيع معتمد | لا اختيار مقياس في الواجهة | ثابت salePrice فقط | لا expenses/receipts |
| تعريف البيع | Orders معتمدة | ApprovalStatus=Approved | — | نفس قاعدة ManagementSummary.SalePrice | معاد استخدام القاعدة لا الـendpoint |
| تاريخ الخلية | SaleDate يوم الشهر؛ إن null فـCreatedAt بحدود يوم بغداد | تقويم تاريخ البيع ليس قطع 03:00 | شهر كامل | `IraqCalendar` DateOnly + fallback CreatedAt | موثّق ≠ SalesWorkDay 03:00 |
| شبكة أيام×فروع | matrix.branches[].days | total لكل يوم/فرع | كل فروع المخوّل | SalesSummary matrix | — |
| إجمالي الفرع/اليوم/الشهر | مجموع الخلايا | على كامل النتائج | — | branchTotals/dayTotals/monthTotal | — |
| نقر خلية | openDrill إن total≠0 | قائمة بيوع اليوم | page | GET details + Drilldown | Drilldown لا يمنح Read |
| صلاحية | قديمًا مراقب افتراضيًا | قراءة مستقلة | — | SalesSummary.Read / .Drilldown | خارج Permissions.All |
| ManagementSummary | صفحة مختلفة | مقاييس متعددة + صناديق | — | **لا** بديل ولا نسخ عنوان | عزل مقصود |

اختبارات: رفض metric غير salePrice؛ مجموع التفاصيل = الخلية؛ SalesSummary.Read ⇏ ManagementSummary.Read؛ عزل فرع.
