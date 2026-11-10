# 08 — مصفوفة مصدر الملاحظات

إثبات: `GetNotesAsync` ← `CustomerNotes`؛ كتابة المتابع عبر `FollowersController.AddCustomerNote` ← `SharedCustomerNotesService`.

| الحقل/الزر | مصدر الكود | قاعدة العمل | المقابل بالجديد | الصلاحية | اختبار القبول |
|---|---|---|---|---|---|
| قائمة الملاحظات | GetNotesAsync | زبائن قوائم التكليف؛ TOP 400؛ ترتيب أحدث أولًا | GET .../notes | FollowerNotes.Read | تظهر ملاحظات النطاق فقط |
| نص الملاحظة | NoteText | نص حر ≤2000 | noteText | Read | كامل في التفاصيل |
| الكاتب | CreatedByName / UserID | اسم الكاتب المخزّن | authorName / authorUserId | Read | يظهر |
| التاريخ | CreatedAtUtc | UTC كتابة | createdAtUtc | Read | يظهر |
| الزبون | CustomerID | مرتبط بالزبون لا بالبيع | customerId/Name/Phone | Read | |
| القائمة | Customer.DelegateID | نطاق العرض بالقائمة | orderListId عند الكتابة + فلتر قوائم Motaba | Read | |
| Source | ثابت customer | نوع ملاحظات الزبون المشتركة | source=customer | — | ليست ملاحظات مندوب |
| زر إنشاء في اللوحة | — | غير موجود | غير منفَّذ | — | لا زر كتابة إداري |
| كتابة ميدانية | AddCustomerNote | المتابع يكتب على زبون في نطاق قائمته | POST app/committee/customers/{id}/notes | FollowerNotes.Write (Motaba) | يسجّل ويظهر في القائمة |
| تعديل/حذف | — | غير مثبت للوحة | غير منفَّذ | — | |
| مرفقات | — | غير موجودة لهذا المسار | لا | — | |
| HasNote (متدنية) | LoadLatestNotesAsync | أي CustomerNotes على CustomerId؛ بلا فترة هبوط | true/false بعد وجود الجدول | Declining.Read للمؤشر؛ المحتوى يحتاج Notes.Read | لا null بعد تنفيذ 08 |

## تمييز الأنواع

- ليست FollowerDelegateNotes (دفعة10).
- ليست ملاحظة مصادقة البيع.
- ليست ملاحظة للمدير (director-notes).
- ليست SalesCustomerNotes الخاصة ببروفايل المبيعات إن اختلفت الدلالة — المسار المعتمد هنا `CustomerNotes` المشتركة.
