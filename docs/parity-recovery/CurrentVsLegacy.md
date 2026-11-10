# CurrentVsLegacy

## مبدأ الفصل
- أعمال المدير/المراقب (FE director/monitor) → لوحة Management/oversight + Sales حسب الصلاحية.
- تطبيق الفرع والمفوض → Management branch/delegate APP_TABS (ترتيب المصدر القديم).
- متابعة المندوبين ≠ متابعة موظفي المبيعات.
- مهام الإدارة (Task) ≠ مهام اللجنة (CommitteeTask) ≠ عمل المتابعين (FollowerWork).
- الأدوار قوالب بدء؛ التفويض النهائي بالعمليات والصلاحيات على السيرفر.

## جدول مطابقة مختصر
انظر Parity_Matrix.xlsx وRouteActionMatrix.md.

## N/A المبرر
| مصدر | السبب |
|---|---|
| main_follower_application | مهام متعددة المحافظات بلا مستهلك Flutter جديد؛ دمجها في committee_officer يخلط الهويتين |
| MA_ShortCompany | غلاف WebView لموقع خارجي |
| FE.backup.database | تشغيل خادم/نسخ؛ ليس شاشة مستخدم في الجديد |
| committee-ratings (جديد معلن) | not_implemented عن قصد حتى قرار منتج |

## اختلافات مقصودة (مشدّدة أمنياً)
| موضوع | قديم | جديد | سبب |
|---|---|---|---|
| صلاحيات | أدوار ثابتة في الراوتر أحياناً | AccessRequirement على الموارد | لا Admin bypass بالاسم |
| نطاق الفرع | غالباً من جلسة/هاردكود | claims + UserBranches في الـhandler | لا فرع من payload |
| Demo | مسارات demo واسعة | يجب ألا يكون fallback عند فشل API | عزل تدريجي؛ sales ما زال فيه DemoOrdersStore — فجوة P1 |

## مداخل مكررة (ليست أخطاء)
- طلبات البيع في تبويب المبيعات وفي workspace الإدارة: نفس البزنس بمداخل صلاحية مختلفة.
- طلبات البيع في bottom nav موظف المبيعات وفي الدرج: مدخل مزدوج متعمد.

## روابط خاطئة مؤكدة في هذه الدفعة
- لا اكتشاف رابط يوجّه لمتابعة مندوبين بدل موظفين أو العكس في registry الحالي (ملاحظات صريحة في navigation.registry).
- التصحيح هنا: توثيق الفجوات؛ لا حذف مداخل ComingSoon.
