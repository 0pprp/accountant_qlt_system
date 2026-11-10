# Coverage 13 — مسارات المتابعين

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم — GET route ليوم بغداد + لوحة markers + قائمة نصية |
| مختبر وحدويًا | نعم (قواعد الموقع المشتركة + vitest sections) |
| مثبت حيًا | ناقص |
| ناقص | لقطات 360/768/1440؛ إثبات يوم كامل من جهاز |
| غير محسوم | لا — بلا polyline مضلل؛ فجوات معلنة |

## العقود
- `GET /api/v1/admin/FollowerTracking/{followerId}/route?branchId=&date=&page=&pageSize=` — FollowerRoute.Read
- ترتيب حسب CapturedAt؛ truncated معلن

## عزل
- Route.Read ≠ Location.Read
- نقاط حسب BranchId وقت الحدث
