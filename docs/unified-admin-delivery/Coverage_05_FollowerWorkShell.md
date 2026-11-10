# Coverage_05 — هيكل عمل المتابعين والبطاقات

تاريخ: 2026-11-08  
إثبات: مطابقة مصدرية (`director-follower-work` + `DirectorFollowerOps` cards). لا استدعاء تشغيل للقديم. **لا تنفيذ أقسام 06–13. لا دفعة 06.**

## الهوية

| | قديم | جديد |
|---|---|---|
| متابع البطاقة | `Users` نوع متابع + `FollowerUserLists` | `User` عبر `OrderList.MotabaId` (تكليف حالي) |
| تسمية منتج اللجنة | تطبيق منفصل | نفس دور Motaba (مسؤول اللجنة) — ليس كيانًا ثانيًا |
| ليس | — | مندوب / موظف مبيعات / ManagementTasks |
| مسار | `/director-follower-work` | Canonical `/management/follower-work`؛ alias عبر `oversight/follower-work` |

## العقود

| Endpoint | تفويض | ملاحظات |
|---|---|---|
| GET `/api/v1/admin/FollowerWork/followers` | `FollowerWork.Read` **أو** صلاحية قسم مسجّلة | query: branchId?, search?, page, pageSize, sort∈{name,rating,lists} |
| عدادات الأقسام الثمانية | — | **غير مرسلة** — لا صفر وهمي |
| مؤشرات phantom على البطاقة | فقط مع `FollowerWork.Read` | availability available/unavailable |

## الصلاحيات

| صلاحية | Director/Monitor seed | يفتح الدليل | يعلن قسمًا جاهزًا |
|---|---|---|---|
| FollowerWork.Read | نعم | نعم | دليل/بطاقات فقط |
| FollowerSalesReview.Read … Route.Read | لا (مسجّلة في OutsideAll) | نعم (هوية دنيا) | لا — الأقسام `not_implemented` |

تعبير الأب: قراءة الدليل أو صلاحية قسم يحتاج الدليل.  
دليل المتابعين لا يمنح ملاحظات/جباية/موقع/مسارات.

## الواجهة

- `FollowerWorkPage` + فلاتر محافظة/بحث/بطاقات
- أقسام داخلية معروضة كـ «غير منفَّذ» — ليست قفل صلاحية
- لا prefetch لتبويب مقفول؛ query keys تشمل session/branch/filters/page
- تغيير المحافظة يلغي `followerId` غير الصالح

## حالة الإغلاق

| بند | حالة |
|---|---|
| مصفوفة الهوية | منفّذ |
| مصفوفة حقول البطاقة | منفّذ |
| API + صلاحيات + قوالب | منفّذ |
| هيكل UI + تنقل ready للأب | منفّذ |
| اختبارات وحدة API/UI | منفّذ |
| الأقسام الثمانية | **not_implemented** |
| لقطات 360/768/1440 | معلّق (جلسة معزولة) |
| دفعة 06 | لم تبدأ |
