# 01 — مصفوفة حقول/أزرار التنقل والصلاحيات (قبل التنفيذ)

إثبات المصدر = مطابقة مصدرية من FE_Company (لا مقارنة حية).  
الفهرس Excel تقريبي وليس إثباتًا.

| الحقل/الزر | مصدر قديم (ملف + symbol) | القاعدة | بيانات الجديد | الصلاحية النهائية | الأثر | اختبار القبول |
|---|---|---|---|---|---|---|
| أب «لوحة الإدارة» | `vertical/index.js` عناوين المدير/المراقب مجمّعة | لا لوحات حسب الدور؛ أب واحد وظيفي | `navigation.registry` id=`management` | anyOf أبناء الجاهزين (قراءة) | يفتح السايدبار إن وُجد ابن مسموح | ابن واحد Read → الأب ظاهر وباقي الجاهز مقفول |
| الملخص | `director-summary` + VerticalNav inject للمدير فقط | default دور مدير؛ ليس قيدًا في الجديد | تبويب `not_implemented` | مؤجّل — لا صلاحية placeholder ready | لا يظهر كقفل صلاحية | Status=not_implemented في السجل |
| التقييمات | `director-ratings` | ظاهر للمدير والمراقب | `not_implemented` | مؤجّل | — | — |
| متابعة المندوبين | `director-delegate-followup` | ظاهر للمدير والمراقب | `/management/director/delegate-followup` | `DelegateFollowup.Read` فقط لدخول التبويب | UpsertBoardRating إجراء منفصل | Update بلا Read → AccessDenied بلا جلب |
| عمل المتابعين (8) | `director-follower-work` + tabs + followers-map/routes | tabs لا تلتف على صلاحية الابن | أبناء صريحون `not_implemented` | مؤجّل لكل ابن | tab tampering مرفوض عند التنفيذ | مسجّل غير منفّذ |
| المهام | `director-assigned-tasks` | مدير في القديم | `/management/.../assigned-tasks` | `Task.Read` لدخول القائمة | إجراءات Create/Assign/… منفصلة | Create بلا Read → لا قائمة (API 403) |
| مهام اللجنة / ملاحظات / تنازلات / قانونية / أسعار / إعلانات / فيديوهات | routes director-* | defaults مدير/مراقب مختلفة | `not_implemented` في السجل | لا placeholder ready | — | — |
| أعمال الفرع/المفوض | موجود في الجديد | الاحتفاظ بلا حذف | أقسام `branch` / `delegate` | صلاحيات التبويبات الحالية | aliases آمنة | الوظائف السابقة تفتح |
| أب «المبيعات» | `إدارة المبيعات` group | أب واحد | `sale` parent | anyOf أبناء الجاهزين | — | — |
| نظرة عامة | `sales-manager-dashboard` | مراقب+مدير مبيعات | `not_implemented` | مؤجّل | — | — |
| طلبات البيع | `sales-manager-requests` | الثلاثة في الراوتر | `/sale/requests` + aliases | تبويبات SalesRequest.* | — | — |
| موظفو المبيعات | `sales-manager-employees` | مراقب+مدير مبيعات | `not_implemented` | مؤجّل | — | — |
| متابعة المبيعات | `director-sales-followup` / sales followup | مندوبون≠موظفو مبيعات | `/sale/followup` | `SalesFollowup.Read` | — | — |
| موقع/مسارات/قانونية/ملخص مبيعات | sales-manager-* | ملخص مبيعات للمراقب فقط (default) | `not_implemented` عدا المبيعات المنجزة=`SaleList` | مؤجّل / `Order.Read` | — | — |
| RoleDefaultMatrix | VerticalNav + router allowlists | قوالب قابلة للتخصيص | أدوار `Director`/`Monitor`/`SalesManager` | بدون ManagePermissions | seed idempotent | seed مرتين لا يمحو Deny |

## غير محسوم (صريح)
- صلاحيات إجراءات التقييمات/عمل المتابعين التفصيلية من السيرفر القديم — تُستخرج في برومبتات المجالات لاحقًا.
- ربط أي دور قديم (ChiefAccountant) بالمدير — **مرفوض تخمينًا**.
