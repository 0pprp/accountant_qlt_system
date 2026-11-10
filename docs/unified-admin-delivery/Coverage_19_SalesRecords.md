# Coverage 19 — المبيعات المنجزة

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم — GET `/admin/SalesRecords` + `/sale/completed` + Order.Read |
| مختبر وحدويًا | نعم — تعريف البيع المعتمد؛ allowlist فرز؛ معادلة الباقي |
| مثبت حيًا | ناقص — لا Browser-live/API-live بهذا التسليم |
| ناقص | تصدير Excel غير موجود في المرجع → غير منفَّذ (فجوة Followup 16 تبقى مفتوحة) |
| غير محسوم | لا |

## تعريف البيع المشمول
- `Order.ApprovalStatus = Approved`
- تاريخ الاحتساب: `Order.SaleDate`
- لا صف من `SalesRequest.Completed` وحده
- لا إنشاء Sale موازٍ؛ `LinkedOrderId` للربط فقط
- SellerId تاريخي يبقى ظاهرًا بعد انتهاء التكليف
- الواصل = مجموع `InstallmentPayments` لهذا الـOrder فقط

## تفاصيل ومستندات
- تفاصيل البيع: مسار SaleView الحالي (`Order.Read`)
- بروفايل الزبون: صلاحية البروفايل + return allowlist (`SaleCompletedRoute`)
- لا أزرار تعديل سعر/حذف بيع على هذه الشاشة

## ملفات
- `Application/SalesRecords/**`
- `Api/.../SalesRecordsController.cs`
- `panel SalesRecords/`

## نواقص سابقة محفوظة (لم تُغلق هنا)
- 10–17 gaps أعلاه تبقى Open
- 16 Excel زبائن Followup: **مفتوح** — تصدير المبيعات غير موجود في المرجع ولم يُنفَّذ كبديل
- 18/19 live verification: Open
- 20/21: لم تبدأ
