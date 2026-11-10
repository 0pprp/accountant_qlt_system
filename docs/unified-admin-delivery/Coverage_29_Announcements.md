# Coverage 29 — الإعلانات

## الحكم
| المحور | الحالة |
|---|---|
| Implemented | نعم — `Announcement` + Admin CRUD/upload + App feed/media + صفحة الإدارة + `delegate_app` بدون seed |
| Unit-tested | نعم — نص/صورة/روابط/concurrency + فصل عن AppNote |
| API-live | **قائمة PASS (32)** — GET list؛ create يحتاج تثبيت عقد الجسم (400 على جسم تجريبي) |
| Browser-live | **Missing** |
| Device-verified | **Missing** |
| Missing | create/publish Browser/Device؛ لا Push (غير موجود في المصدر) |

## حدود المجال
بانر `delegate|follower` + `IsEnabled`. ليس AppNote وليس Notification. لا جدولة ولا إقرار قراءة.

## type-check
نفس مقارنة 28: **20 → 20**؛ لا أخطاء متبقية في `Announcements/`.

لا دفعة 30/31. لا نشر إنتاج.
