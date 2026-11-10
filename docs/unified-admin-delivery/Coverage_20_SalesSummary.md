# Coverage 20 — ملخص المبيعات

## الحالة
| المحور | الحكم |
|---|---|
| Implemented | نعم — GET matrix/details + `/sale/summary` + SalesSummary.Read/Drilldown (OutsideAll) |
| Unit-tested | نعم — 12 اختبارات SalesSummary (+ قواعد اليوم/الفترة/الأخطاء) |
| API-live | نعم — `qalaat_20_21_iso` عبر http://127.0.0.1:4200 |
| Browser-live | نعم — واجهة ضد API حي بلا mock |
| Missing | — |

## إثبات حي (API)
- matrix `2026-11`: `monthTotal=150000`, `metric=salePrice`, dayBound = تقويم SaleDate + fallback بغداد 00:00 (ليس 03:00)
- details لخلية موجبة: `cellTotal=150000` = مجموع الخلية؛ `totalCount=1`
- `metric=expenses` → 400 MetricRejected
- Admin كان يفتقد SalesSummary في RolePermissions — أُصلح بإضافة OutsideAll إلى دور Admin

## إثبات حي (Browser)
- `/sale/summary?year=2026&month=11` يعرض «ملخص المبيعات» وإجمالي ١٥٠٬٠٠٠ IQD
- لقطات: `evidence/20-sales-summary/summary-{360,768,1440}.png`
- JSON: `evidence/20-21-live-api.json`

## قواعد
- مقياس ثابت salePrice فقط؛ رفض أي metric آخر.
- إعادة استخدام قاعدة ManagementSummary.SalePrice دون endpoint الملخص الإداري.
- Drilldown لا يمنح قراءة المصفوفة والعكس.

## نواقص سابقة محفوظة
- 10–19 live gaps تبقى Open حيث وُثّقت سابقًا؛ 22 لم تبدأ.
