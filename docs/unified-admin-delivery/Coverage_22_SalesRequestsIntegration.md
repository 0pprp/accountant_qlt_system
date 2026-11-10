# Coverage 22 — تكامل طلبات البيع

## الحالة
| المحور | الحكم |
|---|---|
| Implemented | إنشاء من 3 تطبيقات؛ Submit idempotent؛ Source/CreatedBy من السيرفر؛ هاتف مطبّع؛ قانونية ميدانية + معاينة مستمسكات لوحة؛ Realtime موجود |
| Unit-tested | SalesRequestSubmission*؛ LegalFollowupCompletionRules؛ Flutter form tests |
| API-live | نعم — `qalaat_22_23_iso` على http://127.0.0.1:4223 (انظر evidence/22-sales-integration/live-api.json) |
| Browser-live | معاينة مستمسكات مضافة في الكود؛ لقطات 360/768/1440 لهذه الشاشة Missing في هذه الجلسة |
| Device-verified | Missing — لا جهاز فعلي مسجّل |
| Missing | Device-verified لأزرار الإنشاء الثلاثة؛ Browser-live لقطات زر الإنشاء |

## أُغلق من نواقص 21 (بدليل كود + اختبار قواعد)
| نقص 21 | الإغلاق |
|---|---|
| Complete ميداني للمتابع | AppArea Field* + صفحات اللجنة + اختبارات الإكمال |
| معاينة مستمسكات الملفات | تخزين LegalFollowupCaseDocuments + Admin/App download + UI معاينة |

## إصلاحات مثبتة في هذه الدفعة
| إصلاح | السبب | اختبار |
|---|---|---|
| تطبيع مسافات الهاتف | القاعدة المشتركة تسمح بتطبيع المسافات ثم الشكل النهائي | `Phone_contract` InlineData بمسافات |
| إزالة «قريباً» من أزرار الإنشاء | الزر كان موجودًا لكن الوسم يوحي بعدم الجاهزية | ربط `NewSalesRequestPage` |

## نواقص سابقة
10–19 الحية الموثقة تبقى Open. لا تُسقط من السجل.
