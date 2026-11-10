# Coverage_03 — الملخص الإداري

تاريخ: 2026-11-08  
إثبات: مطابقة مصدرية من `director-summary` + `DirectorSummaryService`. لا استدعاء API القديم. لا دفعة 04.

## الهوية

| | قديم | جديد |
|---|---|---|
| مسار | `/director-summary` | `/management/summary` (+ `oversight/summary`) |
| صلاحية | UserType مدير + DirectorOrMonitor | `ManagementSummary.Read` / `Drilldown` |
| ملخص المبيعات | شاشة منفصلة | **لا يفتح** هذا الملخص |

## المقاييس

| مفتاح | الحالة | مصدر الجديد | ملاحظة |
|---|---|---|---|
| receipts | منفّذ | InstallmentPayment.Date + Order Approved + !IsLegal | |
| expenses | منفّذ (تكييف) | Expense.TotalAmount · CreatedAt بغداد | ليس TransferBox القديم |
| salePrice | منفّذ | Order.SellAmount · SaleDate/CreatedAt | Approved فقط |
| purchasePrice | منفّذ | Order.BuyAmount لنفس المبيعات | ≠ buys |
| buys | منفّذ | Purchase.TotalAmount · CreatedAt بغداد | فواتير شراء |
| boxTotalAmountDenar | تكييف موثّق | Σ Safe.Remaining للفروع | ليس أبو سجاد/قوائم |
| treasuryAmountDenar | تكييف موثّق | خزين/قاصة أو أعلى رصيد | يتطلب Safe.Read |

## العقود

| Endpoint | صلاحية | ملاحظات |
|---|---|---|
| GET `/api/v1/admin/ManagementSummary/matrix` | Read | year, month, metric, branchId? |
| GET `/api/v1/admin/ManagementSummary/details` | Drilldown | branchId إلزامي + day؛ paginated |

## الصلاحيات والقوالب

| دور قالب | Read | Drilldown | Safe.Read |
|---|---|---|---|
| Director | نعم | نعم | نعم (لبطاقات القاصة) |
| Monitor | لا | لا | — |
| SalesManager | لا | لا | — |

Drilldown بلا Read لا يفتح التبويب. طلب branchId غير مخوّل → رفض.

## غير محسوم / ناقص

- مطابقة حرفية لـ boxTotal (أبو سجاد + نشاط 30ي لصناديق القوائم).
- لقطات تفاعلية 360/768/1440 معلّقة إن لم يُشغَّل السيرفر المعزول في الجلسة.
- فهارس DB إضافية — تُراجع بعد قياس الاستعلام على بيانات أكبر.

## حالة الإغلاق

| البند | الحالة |
|---|---|
| مصفوفة الحقول | docs/03-summary-field-matrix.md |
| API matrix/details | منفّذ |
| وحدة الواجهة + قفل | منفّذ |
| اختبارات قواعد/تقويم/مقاييس | منفّذ (وحدة) |
| دفعة 04 | لم تبدأ |
