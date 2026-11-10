# Coverage 28 — لوحة الأسعار

## الحكم
| المحور | الحالة |
|---|---|
| Implemented | نعم — `PriceBoardEntry` + Admin CRUD + App `PriceBoardAds` + صفحة الإدارة + ربط `sales_employee_app` |
| Unit-tested | نعم — مجال decimal/concurrency + صلاحيات OutsideAll/Monitor |
| API-live | **قائمة PASS (32)** — GET `/admin/PriceBoard` total=0؛ إنشاء/نشر غير مثبت |
| Browser-live | **Missing** |
| Device-verified | **Missing** — الصفحة مربوطة بالكود دون تشغيل جهاز |
| Missing | إنشاء/نشر Browser / Device |

## حدود المجال
إعلانات أسعار نصّية/رقمية حسب `ProvinceId`. **لا** تعدّل Product ولا Order.  
`SalesPriceBoard/price-board` الحالي (منتجات) يبقى مجالًا منفصلًا.

## type-check
- قبل: **20** أخطاء (`typecheck-before-errors.txt`)
- بعد: **20** أخطاء (`typecheck-after-errors.txt`)
- فرق وحدات 28/29: لا أخطاء جديدة بعد إصلاح `AnnouncementsPage` dialog boolean
- الأخطاء الباقية خارج `PriceBoard`/`Announcements` (نفس قائمة الأساس)

لا دفعة 30. لا نشر إنتاج. نواقص 10–27 مفتوحة.
