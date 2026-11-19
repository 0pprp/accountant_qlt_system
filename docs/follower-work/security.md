# الأمان

## 1. طبقات التحقق (كل طلب)

1. **`[HasPermission]` على الـ endpoint:** يُرفض الطلب قبل الـ handler إن لم تكن الصلاحية في التوكن.
2. **داخل الـ handler:** إعادة فحص الصلاحية (`currentUser.HasPermission`)، لأن الـ handler قد يُستدعى من مكان آخر.
3. **عزل الفرع:** `FollowerWorkAccess.CanAccessBranch`. الفرع المطلوب يجب أن يكون في `BranchIds` للمستخدم، وإلا `BranchAccessDenied`.
4. **انتماء المتابع:** `FollowerSalesReviewDataService.ResolveFollowerScopeAsync` يأخذ قوائم المتابع داخل فروع المستخدم فقط. متابع بلا قوائم في النطاق يُرجع `FollowerBranchMismatch` أو `FollowerNotFound` دون كشف بياناته في فرع آخر.
5. **انتماء الزبون:**
   - البروفايل يُرفض (`CustomerOutOfScope`) إن لم يكن للزبون طلب على قوائم المتابع.
   - الصورة تُرفض إن لم يكن للزبون طلب في قائمة من الفرع المطلوب.

## 2. الصلاحيات

`Application/Common/SeedData/Permissions.cs` ← `FollowerWork`

| الصلاحية | ما تفتحه |
|---|---|
| `FollowerWork.Read` | الصفحة وعمود المتابعين |
| `FollowerWork.SalesReviewRead` | مصادقة البيع واللوح الجانبي |
| `FollowerWork.DecliningRead` | المتدنية |
| `FollowerWork.NotesRead` | الملاحظات |
| `FollowerWork.OldCustomersRead` | الزبائن القديمة |
| `FollowerWork.DelegateNotesRead` | ملاحظات المندوبين |
| `FollowerWork.CollectionRead` | قراءة الجباية |
| `FollowerWork.CollectionManageEnabled` | تشغيل/إيقاف الجباية |
| `FollowerWork.LocationRead` | الموقع المباشر + مواقع وصور المحلات + فتح الصور |
| `FollowerWork.RouteRead` | المسارات |
| `FollowerWork.ManageFollowers` | **جديدة:** إنشاء/تعديل/حذف حساب متابع وإسناد القوائم |

- **تبويبات اللوحة (`AdminPanelTabs.cs`):** كل تبويب `follower*` مربوط بصلاحياته. تبويبان جديدان: `followerShopCapture` و`followerManage`.
- **الأدوار الافتراضية (`RoleDefaultMatrix.cs`):**
  - المدير والمراقب: كل تبويبات المتابعين.
  - اللجنة: لا شيء افتراضياً، وتُمنح لكل مستخدم من محرر صلاحيات التبويبات. هذا يغلق عيبي المصدر #9 و#10 بدل الاعتماد على إخفاء الواجهة.

## 3. قيود الكتابة

**تشغيل/إيقاف الجباية:** `PATCH admin/OrderLists/{id}/follower-collection`
- يرسل `expectedFollowerId`. يُرفض (`CollectionListNotFollowers`) إن لم تكن القائمة لهذا المتابع (إصلاح عيب المصدر #7).
- `expectedVersion` يمنع الكتابة فوق تغيير متزامن.

**قفل دخول المندوب (`LoginHandler`):**
- عند دخول التطبيق (`EnforceSalesAppBlock = true`) يُرفض المندوب برسالة `Delegate_Follower_Collection_Locked` ما دامت أي قائمة له مفعّلة للجباية.
- دخول اللوحة غير متأثر.

**إنشاء متابع:**
- `UserAssignmentGuard.EnsureCanAssignRoleAndBranchesAsync`: لا يُنشئ المستخدم حساباً في فرع خارج نطاقه.
- الدور ثابت: Motaba.
- اسم المستخدم ≥ 3 ومكرر = رفض.
- الرمز السري ≥ 8 (القديم كان ≥ 4).
- القوائم المسندة يجب أن تكون في فروع المستخدم (`ListsOutOfScope`).
- العملية في معاملة (transaction) واحدة.

**تعديل متابع:**
- يجب أن يحمل الحساب دور Motaba، وأن تكون كل فروعه ضمن نطاق المستخدم.
- الهاتف أو الرمز الفارغ يُبقي القيمة الحالية.
- رمز جديد يُبطل كل الجلسات (`InvalidateSessions`).

**حذف متابع:**
- يُرفض إن كانت له قوائم (`FollowerHasLists`): انقلها لمتابع آخر أولاً.
- يُرفض إن كان له سجل عمل، أي مصادقات أو نشاط (`FollowerHasHistory`)، حفاظاً على التدقيق.
- أي تعارض مفتاح أجنبي من القاعدة يُعامل كـ`FollowerHasHistory`.

**فتح الصورة:**
- `source` يقبل `imported` أو `app` فقط.
- مسار ملف التطبيق يُحل تحت `wwwroot` ويُرفض أي مسار يخرج منه (حماية path traversal).

## 4. التدقيق

- كل إنشاء/تعديل/حذف حساب يُسجَّل في `ActivityLog`. الحذف مسجّل حالياً بنوع `UserUpdated` مع الوصف «follower deleted»، إذ لا يوجد نوع `UserDeleted` (فجوة معروفة).
- تشغيل الجباية وإيقافها يُسجَّل من الخدمة الموجودة.
- لا أرقام سرية ولا بيانات شخصية في السجلات.
