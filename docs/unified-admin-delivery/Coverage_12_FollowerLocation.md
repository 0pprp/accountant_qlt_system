# Coverage 12 — الموقع المباشر للمتابعين

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم — ingest معزّز + GET latest + لوحة Leaflet + poll |
| مختبر وحدويًا | نعم (`FollowerLocationRulesTests` + vitest) |
| مثبت حيًا | ناقص — لا API-live/Browser-live/Device-verified في هذه الجلسة |
| ناقص | تحقق جهاز ميداني؛ خلفية OS (غير مدّعاة) |
| غير محسوم | لا |

## المصدر
- التتبع مربوط بالدوام (SalesShift).
- التطبيق: `committee_officer_application` → `locations/live` بعد start + كل 60ث في المقدمة.
- القراءة: متابعون = Motaba على OrderLists فقط.

## العقود
- `GET /api/v1/admin/FollowerTracking/latest?branchId=&followerId=` — FollowerLocation.Read
- `POST /api/v1/app/committee/locations/live` — جلسة + دوام نشط

## نواقص 10/11 المحفوظة
- ربط كتابة ملاحظات المندوب في Flutter
- قفل دخول المندوب عند تمكين الجباية
- التحقق الحي لـ10/11
