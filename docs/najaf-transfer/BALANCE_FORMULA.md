# معادلة المتبقي (snapshot النجف)

## المعادلة المثبتة في المصدر

المتبقي المعروض في `View_CustomersRemaining` يطابق غالبًا:

مثبّتة من تعريف `View_CustomersRemaining` على الـsnapshot:

```text
ROUND(
  ISNULL(SUM(View_CustomersSales.AmountTotalSalesDenar), 0)
  - ISNULL(SUM(View_AddToBox.AmountDenar GROUP BY CustomerIDPayment), 0)
, -3)
```

التحصيل يُنسب عبر **`CustomerIDPayment`** (وليس `CustomerID` وحده). التقريب إلى أقرب ألف دينار (`ROUND(..., -3)`).

## نتائج الـsnapshot (NajafRehearsal)

| مقياس | القيمة (د.ع) |
|---|---:|
| زبائن | 5,859 |
| ديون معروضة | 2,072,133,000 |
| دائن معروض | 2,483,000 |
| صافي معروض | 2,069,650,000 |
| مبيعات (View_CustomersSales) | 7,431,076,500 |
| تحصيلات منسوبة (CustomerIDPayment) | 5,361,425,958 |
| صافي خام | 2,069,650,542 |
| فرق الخام − المعروض | **542** |

الفرق ناتج عن تجميع فروق التقريب/الحدود على مستوى الحساب، وليس عن نقص صفوف.

## فحص الـ18 حسابًا

أُعيد تشغيل [`tools/balance_audit.sql`](tools/balance_audit.sql) على الـsnapshot:

- `AccountsDiffHalfDinar = 18`
- `SumRawMinusDisplay` (لهذه الحسابات) = 542
- `SumRawMinusDisplay_All` (كل الزبائن) = 542
- عيّنة: فروق غالبًا 500 أو أقل عند حدود التقريب للألف؛ حساب واحد بفرق 8 د.ع (ضوضاء عشرية).

المخرجات على السيرفر (خارج Git):  
`/root/najaf-transfer-BluaWosX/balance_audit_out.txt`

## قاعدة المطابقة مع الهدف

- الهدف يخزّن المبالغ بدون إعادة تطبيق `ROUND(..., -3)` على كل عملية ترحيل.
- تقرير المطابقة يعرض: **صافي خام المصدر** و**صافي معروض المصدر** و**صافي الهدف** منفصلين.
- فرق 542 مفسَّر مسبقًا؛ أي فرق إضافي غير مفسَّر يمنع القطع.
