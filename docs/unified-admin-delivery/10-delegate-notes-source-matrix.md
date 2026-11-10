# 10 — مصفوفة مصدر ملاحظات المندوبين

إثبات: `GetDelegateNotesAsync` ← `FollowerDelegateNotes`؛ الكتابة `FollowersController.AddDelegateNote` (دور CreatedByRole = Follower).

| الحقل/الزر | مصدر الكود | قاعدة العمل | المقابل بالجديد | الصلاحية | اختبار القبول |
|---|---|---|---|---|---|
| من يكتب؟ | AddDelegateNote + RoleFollower | **المتابع** يكتب ملاحظة عن مندوب/قائمة | Motaba عبر POST committee | FollowerDelegateNotes.Write | ليس المندوب |
| الكيان | DelegateId=ListId | ملاحظة على القائمة/مندوبها لا على زبون | OrderListId (+ MandobId مشتق) | — | ليست CustomerNotes |
| العرض الإداري | WHERE CreatedByUserId=Fid OR ListId IN lists | تكليف حالي + ملاحظات كتبها المتابع | GET .../delegate-notes | FollowerDelegateNotes.Read | قراءة فقط باللوحة |
| تعديل/حذف | NotePutForbidden | ممنوع بعد الحفظ | غير منفَّذ | — | 403 |
| تمييز عن 08 | جدول منفصل | FollowerDelegateNotes ≠ CustomerNotes | كيان مستقل | — | لا خلط |

**لا:** انتحال CreatedBy من العميل؛ الكتابة على قائمة غير مسندة للمتابع.
