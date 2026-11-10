# 29 — مصفوفة مصدر الإعلانات

مرجع استكشاف: [استكشاف الإعلانات](3637d243-6dd2-44c9-8c20-94e90a8aa0cd).

## تعريف المجال (من المصدر)
**محتوى بانر للصفحة الرئيسية** موجّه بمفتاح تطبيق (`delegate` | `follower`) — نص XOR صورتان (بانر+تكبير) + رابط اختياري + `IsEnabled` + `SortOrder`.

**ليس:** Push، AppNote، Notification، لوحة الأسعار (رغم أن واجهة الأسعار تستخدم كلمة «إعلان»).

لا عنوان منفصل، لا جدولة، لا إقرار قراءة، لا استهداف فرع، لا إشعار عند النشر — التسليم = سحب القائمة.

## الفاعلون
| الطرف | إجراء |
|---|---|
| المدير (المصدر) | CRUD + رفع صور + تفعيل |
| تطبيقات المندوب/المتابع | قراءة العامة المفعّلة لمفتاحهم |

## المسار
`director-announcements.vue` → `api/sales-manager/announcements` → `AppAnnouncementService` → `dbo.AppAnnouncements` + ملفات تحت `App_Data/announcements`  
المستهلك: `GET /api/mobile-announcements/{appKey}` (قديمًا AllowAnonymous).

## الأمان
- نص عادي (لا HTML).
- روابط http/https فقط.
- صور jpg/png/webp/gif ≤8MB.
- عزل الجمهور على السيرفر بـ AppKey.

## المقابل في الجديد / الفجوة
| عنصر | الحالة |
|---|---|
| تبويب الإدارة | stub → يُنفَّذ |
| كيان Announcement | غير موجود → يُنشأ |
| AppNote / Notification | موجودان ومنفصلان — لا دمج |
| delegate_app carousel | seed ثابت → يُربط بالعقد |
| follower app في الجديد | غير موجود — عقد API جاهز |
| Push عند النشر | غير موجود في المصدر — لا يُختلق |
| API-live / Browser-live | Missing حتى الإثبات |
