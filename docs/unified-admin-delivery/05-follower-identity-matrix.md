# 05 — مصفوفة هوية المتابع والتكليف

إثبات: مطابقة مصدرية (`FollowerDirectoryService` + `FollowerUserLists` + `director-follower-work.vue`)  
مقابل الجديد: `User` + دور `Motaba` + `OrderList.MotabaId`.  
ليست SalesFollowup ولا ManagementTasks ولا مندوب (`MandobId`).

## المفاهيم

| المفهوم القديم | المفتاح القديم | المقابل الجديد | العلاقة | الدليل | النقص |
|---|---|---|---|---|---|
| متابع (بطاقة عمل المتابعين) | `Users.UserID` حيث `UserType=متابع` و`UserState=1` | `User.Id` بدور **Motaba** | حساب مستخدم | `FollowerDirectoryService` | لا تعتمد على اسم العرض فقط |
| تكليف القوائم | `FollowerUserLists (UserId, ListId)` | `OrderList.MotabaId = User.Id` | تكليف **حالي** على القائمة | Coverage_02 + OrderList | لا جدول ACL منفصل في الجديد |
| فرع العمل | مدينة المستخدم / قوائم التكليف | `OrderList.BranchId` للقوائم المسندة (+ `UserBranches` للنطاق) | فرع عبر القوائم | المصدر: fan-out مدينة | لا «كل قوائم الفرع لكل متابع» |
| مندوب | `Delegates` / Mandob | `OrderList.MandobId` | مختلف | — | لا خلط |
| موظف مبيعات | Seller / SalesDrafts EmployeeId | `Order.SellerId` | مختلف | — | لا خلط |
| مسؤول اللجنة (تطبيق اللجنة) | تطبيق قديم مختلف المسار | نفس دور **Motaba** في الجديد (تسمية منتج: مسؤول اللجنة) | نفس حساب التكليف عبر MotabaId | `committee_officer_application/AGENTS.md` | تسمية منتج ≠ كيان منفصل |
| نشاط الدليل | `UserState=1` | وجود دور Motaba + قوائم مسندة ضمن الفروع المسموحة | «نشط عمليًا» = له تكليف في النطاق | — | لا UserState منفصل موثّق |
| نقل/إنهاء تكليف | حذف صف FollowerUserLists | تغيير/تفريغ `OrderList.MotabaId` | فوري | — | بلا تاريخ تكليف محفوظ — **موثّق** |
| وردية / متصل | `HasActiveShift` في الدليل (غالبًا 0) | لا مصدر وردية ميدانية مثبت لهذه الشاشة | — | Directory يضع 0 | **لا شارة متصل**؛ لا login كبديل |
| تقييم البطاقة الوهمي | متوسط وهمي لقوائم المتابع النشطة 30ي | نفس `CustomerRatingCalculator` + phantom | عبر قوائم MotabaId | `GetFollowerCardsAsync` | منفّذ في دليل البطاقات |
| عدادات الأقسام (ملاحظات/جباية/…) | `DirectorFollowerOpsSummaryDto` | مصادر أقسام 06–13 | — | summaries | **غير مرسلة** في 05 (أقسام not_implemented) |

## قواعد ثابتة

1. ظهور البطاقة: مستخدم Motaba وله ≥1 قائمة `MotabaId` ضمن فروع الحساب المسموحة (بعد فلتر `branchId` إن وُجد).
2. لا يكفي وجود حساب في الفرع بلا تكليف قائمة.
3. لا يكفي `followerId` صحيح بلا علاقة فرع↔تكليف.
4. تكليف حالي فقط — لا محاكاة تاريخية للبطاقات.
5. تعارض التسمية: «متابع» في لوحة الإدارة = Motaba؛ تطبيق اللجنة يسميه مسؤول اللجنة لنفس الدور — وثّق ولا تُنشئ كيانين.

## غير محسوم

- سجل تاريخي لتكليف القوائم (من→إلى) — غير موجود بالجديد؛ أي تقرير تاريخي يحتاج تصميم لاحق.
- وردية المتابع الحية لشارة IsWorking — بلا مصدر موثوق هنا.
