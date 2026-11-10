# 17 — مصفوفة مصدر الموقع المباشر لموظفي المبيعات

مصدر: `sales-manager-map.vue` + ingest تطبيق الموظف.

| العنصر | المصدر | المقابل الجديد |
|---|---|---|
| بدء الإرسال | بعد بدء دوام | SalesShift نشط + تكليف مبيعات |
| توقف | نهاية الدوام / خروج | لا دوام → لا ingest |
| خلفية | قديم محدود | **مقدمة فقط** في التطبيق الحالي — موثّق |
| تكرار | ~live poll | App live؛ لوحة poll 15ث |
| capturedAt / receivedAt | نعم | RecordedAtUtc / ReceivedAtUtc |
| accuracy | نعم | AccuracyMeters nullable |
| تخزين | نقاط GPS | SalesLocationPoints + ActorKind=SalesEmployee |
| قراءة لوحة | live-locations | GET /admin/SalesEmployeeTracking/latest |
| صلاحية | SM | SalesLocation.Read (مستقلة عن FollowerLocation) |
| عزل المتابع | خرائط منفصلة | ActorKind + قائمة تكليف؛ Follower يستبعد SalesEmployee |

لا مسار تاريخي (18). لا إحداثيات زبون. لا Hub موازٍ.
