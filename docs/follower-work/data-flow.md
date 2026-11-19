# تدفق البيانات

المبدأ: الشاشة **تقرأ فقط** ما تكتبه التطبيقات، بنفس تسلسل القديم. الكتابة من اللوحة محصورة في ثلاثة أشياء:
- تشغيل وإيقاف الجباية.
- حسابات المتابعين وإسناد القوائم.
- إلغاء زبون قديم (من قسم الزبائن القديمة).

## 1. من يكتب ماذا

| الكاتب | الـ endpoint | الجدول / الحقل | يظهر في |
|---|---|---|---|
| **تطبيق المتابع** (تطبيق اللجنة) | `POST app/Committee/pending-sales/{orderId}/authenticate` | `FollowerSaleAuthentications` + `CustomerStaffRating` بدور `CommitteeField` | مصادقة البيع: «مصادق»، وصندوق «تقييم المتابع» |
| تطبيق المتابع | `POST app/Committee/customers/{customerId}/location` | `Customer.Business` (GPS) + `Order.Location` | مواقع وصور المحلات: «له موقع» |
| تطبيق المتابع | `POST app/Committee/lists/{listId}/customers/{customerId}/photo` | `Attachments` بنوع `CustomerShopPhoto` (ملف على القرص) | مواقع وصور المحلات: «له صورة» |
| تطبيق المتابع | `POST app/Committee/customers/{customerId}/notes` | `CustomerNotes` | الملاحظات |
| تطبيق المتابع | `POST app/Committee/order-lists/{listId}/delegate-notes` | `FollowerDelegateNotes` | ملاحظات المندوبين |
| تطبيق المتابع | `POST app/Committee/locations/batch`، `locations/live`، `shifts/*` | `SalesLocationPoints` + الدوام | الموقع المباشر، المسارات |
| تطبيق المتابع والمندوب (الدفع) | مسار الأقساط | `InstallmentPayments` (`CreatedBy` = المحصّل) | الواصل والتقييم والجباية |
| **موظف المبيعات** | مسار البيع | `Orders` (`SaleDate`، `OrderListId`، `ExecutionStatus`) | الحقائق، «بانتظار» |
| **الاستيراد من النظام القديم** | سكربت الترحيل | `CustomerStaffRating` بدوري `Delegate` و`SalesEmployee` + `CustomerShopProfiles` | صندوقا «تقييم المندوب» و«موظف المبيعات» (**فجوة:** لا يوجد حالياً كاتب لهما في هذا الـ API، فالبيانات الجديدة لا تظهر حتى يُضاف كاتب) |
| **الإدارة** (هذه الشاشة) | `PATCH admin/OrderLists/{id}/follower-collection` | `OrderList.FollowerCollectionEnabled/AssignedAtUtc` | الجباية + قفل دخول المندوب |
| الإدارة (هذه الشاشة) | `POST/PUT/DELETE admin/FollowerWork/followers[/{id}]` | `Users` بدور Motaba + `OrderList.MotabaId` | عمود المتابعين |
| **الاستيراد من النظام القديم** | سكربت الترحيل | `CustomerShopPhotos` (بايتات داخل القاعدة) | مواقع وصور المحلات (الصور القديمة) |
| شاشة المتدنية نفسها | `GET {followerId}/declining` (عند أول فتح في اليوم) | `FollowerCustomerRatingSnapshots` (لقطة اليوم) | المتدنية |

## 2. من الجداول إلى الأرقام

```
Orders (معتمدة، على قوائم المتابع) ─┐
InstallmentPayments (حتى تاريخ الحساب)─┼─► FollowerFactsQuery ─► RatingCustomerFact لكل (زبون، قائمة)
Customers.IsLegal ─────────────────────┘                     │
                                                              ├─► ActiveRatingCohort (باقٍ > 0 + تسديد ≤ 30 يوماً، تاريخ وهمي)
                                                              │     ├─► كروت القوائم + التقييم الوهمي للمتابع
                                                              │     └─► كروت الفئات الخمس
                                                              └─► كروت الباقي (<100k، 100k–200k)
```

- **«أمس» و«قبل أسبوع»:** نفس الاستعلام بـ`asOfCutoff`. لا تدخل المبيعات بعد ذلك اليوم، ولا التسديدات بعده.
- **«بانتظار»:** طلبات القائمة بـ`ExecutionStatus = NotStarted` ولا مصادقة لها. نفس شرط `GET app/Committee/pending-sales`، فما يراه المتابع في تطبيقه هو ما تراه الإدارة.
- **«مصادق»:** `FollowerSaleAuthentications` على القوائم، سجل واحد لكل طلب.

## 3. الـ endpoints ← الشاشة

القاعدة: `api/v1/admin/FollowerWork`

| الواجهة | الـ endpoint |
|---|---|
| عمود المتابعين | `GET followers?branchId=&search=&page=&pageSize=` |
| عدّادات التبويبات | `GET {followerId}/section-counts?branchId=` |
| مصادقة البيع | `GET {followerId}/sales-review`، `GET {followerId}/remaining-customers?bucket=` |
| المتدنية | `GET {followerId}/declining` |
| الملاحظات | `GET {followerId}/notes` |
| الزبائن القديمة | `GET {followerId}/old-customers` |
| ملاحظات المندوبين | `GET {followerId}/delegate-notes` |
| الجباية | `GET {followerId}/collection` + `PATCH admin/OrderLists/{id}/follower-collection` |
| اللوح الجانبي | `GET {followerId}/customers/{customerId}/profile?branchId=` |
| مواقع وصور المحلات | `GET shop-capture?branchId=&followerId=`، `GET shop-capture/photos/{photoId}?branchId=&source=imported\|app` |
| الموقع المباشر / المسارات | `GET admin/FollowerTracking/latest`، `GET admin/FollowerTracking/{followerId}/route?date=` |
| إدارة الحساب | `GET followers/assignable-lists?branchId=`، `POST followers`، `PUT followers/{id}`، `DELETE followers/{id}` |

## 4. الواجهة

- **الطلبات:** `src/modules/FollowerWork/requests/queries/useFollowerBoardQueries.ts` (TanStack Query).
- **المفاتيح:** تبدأ بـ`FOLLOWER_WORK_KEY`.
- **بعد تشغيل/إيقاف الجباية:** تُبطَل `section-counts` وقائمة الجباية.
- **بعد حفظ حساب:** تُبطَل قائمة المتابعين.
- **الحالة:** في الرابط (`branchId`، `followerId`، `tab`، `customerId`)، لا في store عام.

## 5. الصور: مصدران

| المصدر | الجدول | `photoSource` |
|---|---|---|
| صور النظام القديم | `CustomerShopPhotos` (بايتات) | `imported` |
| صور تطبيق المتابع | `Attachments` بنوع `CustomerShopPhoto` (ملف تحت `wwwroot`) | `app` |

لكل زبون تُعرض **الأحدث** بين المصدرين؛ عند التساوي تُقدَّم صورة التطبيق. القاعدة في `FollowerShopPhotoSources.LatestPerCustomer`.
