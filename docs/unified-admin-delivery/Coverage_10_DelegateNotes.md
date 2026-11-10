# Coverage 10 — ملاحظات المندوبين

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم (كيان + API قراءة/كتابة ميدانية + لوحة قراءة) |
| مختبر وحدويًا | نعم (`FollowerDelegateNotesAndCollectionRulesTests` + vitest sections/nav) |
| مثبت حيًا | ناقص — لم يُنفَّذ API-live / Browser-live / Device-verified في هذه الجلسة |
| ناقص | ربط UI تطبيق المتابع (Flutter) بـ POST الجديد — API جاهز؛ التحقق على الجهاز ناقص |
| غير محسوم | لا |

## المصدر المثبت
- الكتابة: `FollowersController.AddDelegateNote` — CreatedByRole=Follower؛ الكاتب من جلسة المتابع.
- العرض الإداري: `GetDelegateNotesAsync` على `FollowerDelegateNotes`.
- ليست `CustomerNotes` (08).

## العقود
- `GET /api/v1/admin/FollowerWork/{followerId}/delegate-notes` — `FollowerDelegateNotes.Read`
- `POST /api/v1/app/committee/order-lists/{listId}/delegate-notes` — `FollowerDelegateNotes.Write`
- لا تعديل/حذف بعد الحفظ.

## عزل
- صلاحية القراءة لا تمنح الكتابة.
- لا خلط مع ملاحظات الزبون.
- الكاتب/الدور من السيرفر؛ القائمة يجب أن تكون مسندة للمتابع.
