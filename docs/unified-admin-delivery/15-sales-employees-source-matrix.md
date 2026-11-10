# 15 — مصفوفة مصدر موظفي المبيعات

مصدر: `sales-manager-employees.vue` ← `employees` + `rating-trend` / `rating-customers`.

| الحقل/الزر | مصدر | المعنى | المقابل الجديد |
|---|---|---|---|
| من هو الموظف؟ | Users UserType=موظف مبيعات | حساب ميداني | دور Mandob ضمن الفروع المسموحة (ليس Motaba) |
| Active / NoShift | SalesWorkShifts | دوام نشط غير منتهٍ | SalesShifts.EndedAtUtc == null |
| الموقع | LiveLocations + Resolve | Live/Stale/NoShift/NoLocation | SalesLocationPoints + FollowerLocationRules (نفس العتبات) |
| التقييم الحقيقي/الوهمي | stats attach | متوسط درجات الزبائن | CustomerRatingCalculator + ActiveRatingCohort (حقيقي/وهمي) |
| فئات ممتاز…سيء | BUCKETS | تصنيف الدرجة | LevelName / SalesFollowupRules labels |
| زر التقييم | openRating | تفاصيل اتجاه | GET .../SalesEmployees/{id}/rating-summary |
| زبائن الفئة | rating-customers | قائمة زبائن الفئة | ضمن الملخص؛ مبالغ فقط إن صلاحية مالية موجودة وإلا محجوبة |
| حظر/CRUD حساب | غير في هذه الشاشة | — | لا؛ ManageAppBlock عبر SalesFollowup |

**علاقة SellerId:** مبيعات Order.SellerId = هوية الموظف عند وجود بيع؛ القائمة تشمل من بلا مبيعات أيضًا.

## اختبار القبول (مختصر)
| سيناريو | متوقع |
|---|---|
| بلا دوام | ShiftStatus=NoShift؛ Location=NoShift |
| بلا مبيعات | يظهر في القائمة؛ تقييم — |
| فرع جزئي لموظف متعدد الفروع | VisibleBranchIds ∩ scope فقط |
| shiftState=Offline | InvalidShiftState |
| ID خارج النطاق | NotFound |
| قراءة فقط | لا أزرار كتابة/حظر |
| مبالغ الفئة بلا صلاحية مالية | AmountsRedacted |

## تمكين مستقل
`saleEmployees` ready منفصل عن overview. لا 16/17.
