# 12 — مصفوفة مصدر الموقع المباشر للمتابعين

| الحقل | مصدر الكود | القاعدة | المقابل بالجديد |
|---|---|---|---|
| الشخص المتتبَّع | FollowerTracking + Motaba | متابع (مسؤول لجنة) لا مندوب جباية ولا موظف مبيعات | UserId جلسة اللجنة + تصفية MotabaId على OrderLists |
| بدء الإرسال | Start shift + GPS permission | بعد منح إذن الموقع وبدء الدوام | `shifts/start` ثم `locations/live` |
| توقف | End shift / cutoff 03:00 Baghdad | لا تتبع بلا دوام نشط | `shifts/end`؛ القراءة الإدارية تتطلب نقطة حقيقية |
| الحقول | lat/lng/accuracy/capturedAt | accuracy اختيارية؛ لا صفر وهمي | SalesLocationPoint + AccuracyMeters/Captured/Received |
| آخر موقع | max(CapturedAt) ليس ReceivedAt | نقطة قديمة متأخرة لا تستبدل الأحدث | ترتيب حسب RecordedAtUtc (التقاط) |
| الحالة | Live≤150s / Stale / NoShift / NoLocation | لا استنتاج «إذن مرفوض» من الفراغ | FollowerLocationStatus |
| الفرع | من الدوام وقت الحدث | لا branchId من العميل كتفويض | Shift.BranchId عند الحفظ |
| الخريطة | Mapbox في القديم | markers فقط لمن لديهم إحداثيات | Leaflet+OSM (بدون مفتاح سري) |
| التحديث | poll 15s | لا SignalR إحداثيات | poll 15s على latest |

**لا:** خلط مع Sales employee live؛ login كدليل موقع؛ مركز محافظة كموقع شخص.
