# 03 — مصفوفة حقول الملخص الإداري

إثبات: مطابقة مصدرية من `DirectorSummaryService` + FE `director-summary.vue`.  
لا مقارنة حية. لا دمج مع `sales-manager-sales-summary`.

## هوية الشاشة
| | قديم | جديد |
|---|---|---|
| مسار | `/director-summary` (المدير فقط في الواجهة) | `/management/summary` |
| صلاحية | UserType + DirectorOrMonitor API | `ManagementSummary.Read` / `Drilldown` — بلا شرط اسم دور |
| نطاق الفروع | fan-out مدن Qalaa (1–6,8–14) مع استبعاد hardcoded | **فروع الحساب المسموحة فقط** — لا أسماء مدن/قواعد hardcoded |

## المقاييس

| العرض | مفتاح | المعنى (مصدر) | تاريخ الاحتساب | داخل | مستثنى | المقابل الجديد | صلاحية | اختبار |
|---|---|---|---|---|---|---|---|---|
| الواردات | receipts | Σ دفعات الزبائن غير القانونيين | `PaymentDate` / InstallmentPayment.Date | دفعات طلبات معتمدة | Customer.IsLegal | InstallmentPayments ⋈ Orders Approved ⋈ !IsLegal | ManagementSummary.Read | يدوي يوم صفر/قيمة |
| الصرفيات | expenses | تحويلات من خزينة المحافظة لصناديق «صرف» | DateCreate | تحويلات ToBox صرف | بلا خزينة → فارغ | **Expense.TotalAmount** حسب يوم CreatedAt بغداد — مقابل كيان الصرفيات في الجديد (ليس TransferBox) | Read | عينة Expense |
| سعر البيع | salePrice | Σ AmountTotalSalesDenar | DateCreate للمبيعات | مبيعات موجودة | المحذوف/الملغى بـDELETE | Order.SellAmount · يوم SaleDate (أو CreatedAt بغداد) · Approved | Read | طلبان في يوم |
| سعر الشراء | purchasePrice | Σ AmountTotalCostDenar من **نفس مبيعات الزبائن** (COGS) | DateCreate مبيعات | بنود تكلفة البيع | ليس المشتريات | Order.BuyAmount لنفس طلبات البيع | Read | يختلف عن buys |
| المشتريات | buys | Σ View_Buys حيث BuyState نشط | DateCreate | BuyState null/1 | BuyState=0 | Purchase.TotalAmount · يوم CreatedAt بغداد | Read | Purchase نشط |

**purchasePrice ≠ buys** — الأول تكلفة بضاعة مباعة للزبائن؛ الثاني فواتير شراء الموردين.

## أرصدة القاصة (بطاقات دائمة — ليست مقياس مصفوفة)

| العرض | مفتاح | المعنى المصدر | المقابل الجديد | ملاحظة |
|---|---|---|---|---|
| المبلغ الفعلي | treasuryAmountDenar | صندوق «خزينة المحافظة» فقط | Safe رئيسي للفرع: أكبر Remaining أو اسم يحتوي خزين/قاصة | متطلب Safe.Read لعرضه؛ وإلا Unavailable |
| المبلغ الكلي | boxTotalAmountDenar | خزينة + أبو سجاد + صناديق قوائم نشطة 30ي | مجموع RemainingCashAmount لكل Safes الفرع | **تكييف موثّق**: لا أبو سجاد/قوائم Boxes بنفس الشكل؛ لا دمج معاني مختلفة صامتًا |

## غير محسوم
- تطابق تام لصيغة boxTotal القديمة (أبو سجاد + نشاط 30ي للقوائم) — غير منقول حرفيًا.
- منطقة زمنية أعمدة DateCreate في الإنتاج القديم (UTC؟) — الجديد يحوّل CreatedAt إلى تاريخ بغداد صراحةً.
