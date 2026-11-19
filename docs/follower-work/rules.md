# قواعد الأرقام والكروت

- **مسارات المصدر:** نسبية إلى `SalesHaider-main/SalesHaider-main` على `origin/main`. الاختصارات:
  - **SVC** = `BE_Company/Sales/Services/DirectorFollowerOpsService.cs`
  - **DFW** = `FE_Company/src/pages/director-follower-work.vue`
- **مسارات الجديد:** نسبية إلى `api-main/api-main/src/Application`.
- **«اليوم»:** تاريخ بغداد (`Asia/Baghdad`، UTC+3) في الطرفين.

## القواعد المشتركة

| القاعدة | المصدر | الجديد |
|---|---|---|
| **مجموعة التقييم (cohort):** زبائن قوائم المتابع، باقٍ > 0، وآخر تسديد خلال 30 يوماً من تاريخ الحساب. القانونيون غير مستبعدين. | SVC:1550-1563، `ActiveCustomerRules.cs:10-21` | `SalesRatings/Common/ActiveRatingCohort.Build` (`requireRecentPayment: true`)، `ActiveCustomerRules.IsActive` |
| **الحقائق:** مجموع البيع والواصل وآخر تسديد لكل زبون في كل قائمة. الواصل يشمل دفعات الزبون **غير المخصصة لطلب** (كالمصدر: التحصيل يُنسب للزبون `CustomerIDPayment`)، وتُضاف لقائمة آخر بيع له. | `RatingDataSource.cs:20-48` (Views)، `docs/najaf-transfer/BALANCE_FORMULA.md` | `FollowerWork/Common/FollowerFactsQuery.LoadAsync`: الطلبات المعتمدة على القوائم، تجميع بالزبون والقائمة + `InstallmentPayments` بـ`OrderId = null` |
| **درجة الزبون:** المعدّل = الواصل×1000 ÷ (المجموع × الأيام منذ البيع شاملاً). ≥6.667 ممتاز +10، ≥6.25 جيد +5، ≥5.556 ضعيف 0، ≥5 متلكئ −5، وأقل من ذلك سيء −10. | `CustomerRatingModels.cs:147-183` | `SalesRatings/Common/CustomerRatingCalculator.ScoreFor` |
| **التاريخ الوهمي للبيع:** قبل 2026-05-01 يُحسب 05-01. من 05-01 إلى 08-31 يُضاف 30 يوماً. ما بعده بلا تغيير. | `PhantomSaleDate.cs:9-41` | نفس القاعدة، `usePhantomSaleDate: true` |
| **متوسط القائمة ← وصف:** ≥7 ممتاز، ≥3 جيد، ≥0 ضعيف، >−7.5 متلكئ، وإلا سيء. | `ListRatingAggregator.cs:71-78` | `ListRatingAggregator` / `ActiveRatingCohort.Aggregate` |
| **أسهم الاتجاه:** للدرجة «صعود» إذا الفرق > 0.01. العدد يُقارن حرفياً. بلا قيمة سابقة = ثابت. | `DirectorFollowerOpsTrendRules.cs` | `FollowerWorkRules.ComputeScoreTrend` / `ComputeCountTrend` |

## عمود المتابعين

| الرقم | المصدر | الجديد |
|---|---|---|
| **المتابعون:** المستخدمون النشطون من نوع «متابع». | `FollowerDirectoryService.cs:39-63` | مستخدمون لهم قوائم (`OrderList.MotabaId`) في فروع المستخدم: `FollowerWorkDataService` |
| **التقييم الوهمي:** متوسط متوسطات القوائم، مقرّب لمنزلتين. | SVC:382-454 | `FollowerWorkDataService` + `FollowerFactsQuery.NonEmptyListAverages`. **فرق:** القوائم الفارغة لا تدخل المتوسط (انظر differences.md #1) |
| **اتجاه التقييم:** مقارنة بنفس الحساب قبل 7 أيام. | SVC:402-411 | حقائق حقيقية حتى قبل أسبوع: `FollowerFactsQuery` مع `asOfCutoff = اليوم − 7` (differences.md #2) |
| **زبائن التقييم:** مجموع زبائن التقييم في قوائمه. | SVC:441 | نفسه. في الجديد للقائمة متابع واحد، فلا يتكرر العدّ |

## تبويب مصادقة البيع

| الكارت | المصدر | الجديد |
|---|---|---|
| **كروت القوائم:** متوسط وهمي، واتجاه مقابل قبل أسبوع، وعدد الزبائن. | SVC:483-503 | `FollowerSalesReviewDataService` (أسطر 100-101) |
| **كروت الفئات الخمس:** عدّ cohort اليوم، والاتجاه مقابل cohort أمس. | SVC:505-516 | نفس الملف (112-113). حقائق أمس محسوبة حتى أمس فعلاً |
| **الباقي:** «أقل من 100,000» (0 < الباقي < 100,000) و«من 100,000 إلى أقل من 200,000». الاتجاه مقابل أمس. | SVC:519-534 | `FollowerSaleAuthRules.RemainingBand100k/200k`، الأسطر 116-117 |
| **قائمة الأسماء عند الضغط على كارت:** ضغطة ثانية تخفيها. | SVC:560-623 | `GET {followerId}/remaining-customers?bucket=`، والضغط على اسم يفتح اللوح الجانبي |
| **مصادَق:** كل `FollowerSaleAuthentications` على قوائم المتابع، أي متابع، كل الأوقات، الأحدث أولاً، سجل واحد لكل بيع. | SVC:625-805، `DirectorFollowerSaleAuthDedupeRules.cs:18-29` | `FollowerSalesReviewDataService` (~231)، `FollowerSaleAuthRules` |
| **بانتظار:** زبائن القوائم الذين آخر بيع لهم من موظف مبيعات (Completed/Posted) ولا مصادقة لهم، بحد 300. | SVC:625-805 | طلبات القائمة بـ`ExecutionStatus = NotStarted` وبلا سجل مصادقة، الأحدث أولاً، بحد 300 (`.Take(300)`). **نفس قاعدة تطبيق المتابع** |
| **الشارة:** كهرماني = بانتظار، أخضر = مصادق، أحمر إذا تقييم المصادقة أو المحل بين 1 و6. | DFW:503-521، `DirectorFollowerSaleAuthRiskRules.cs` | `FollowerSaleAuthRules` (IsLowRisk) |
| **«التقييم الحالي» على كارت المصادقة.** | حقيقي بلا تاريخ وهمي، SVC:1153-1164 | بالتاريخ الوهمي (أسطر 303، 343) ليطابق الفئات (differences.md #4) |

## الصناديق الأربعة (اللوح الجانبي)

`GET {followerId}/customers/{customerId}/profile`، والملف `Queries/Admin/GetCustomerProfile/GetFollowerCustomerProfileHandler.cs`

| الصندوق | المصدر (DFW:1403-1513) | الجديد |
|---|---|---|
| **عام** | الهاتف، القائمة، الحالة، التقييم، السعر، تاريخ البيع، القسط، البائع، الواصل، الباقي | نفس الحقول + العنوان ورابط الموقع إن وُجد |
| **تقييم المتابع** | آخر مصادقة: التقييم، الملاحظة، قيمة البضاعة، الدخل اليومي، الوقت | آخر `FollowerSaleAuthentication` للزبون على قوائم المتابع |
| **تقييم المندوب** | `DelegateFirstCollectionEvaluations`، وإلا `SalesRequests.ShopRating` | `CustomerStaffRating` بدور المندوب |
| **موظف المبيعات** | `SalesShopProfiles` | `CustomerStaffRating` بدور موظف المبيعات + `CustomerShopProfiles` |

## المتدنية

| القاعدة | المصدر | الجديد |
|---|---|---|
| **الانتقالات المحسوبة:** ممتاز←ضعيف، جيد←متلكئ، ضعيف←سيء فقط، مع هبوط ≥10 نقاط خلال ≤7 أيام. | `DirectorFollowerOpsDeclineRules.cs:11-69` | `FollowerDeclineRules.IsDeclining` (`LookbackDays = 7`، `MinScoreDrop = 10`) |
| **المصدر:** لقطات `FollowerCustomerRatingSnapshots`. | SVC:807-898 | نفس الجدول. الشاشة تلتقط لقطة اليوم إن لم توجد (`CaptureTodaySnapshotsAsync`)، والنافذة تُقاس من اليوم (differences.md #5) |

## الملاحظات، القدامى، ملاحظات المندوبين

| القسم | المصدر | الجديد |
|---|---|---|
| **الملاحظات** | TOP 400 من `CustomerNotes` على القوائم، بشرط فعلي يُرجع كل ملاحظة لها كاتب | `FollowerNotesDataService`: كل ملاحظات زبائن قوائم المتابع، مرقّمة صفحات (differences.md #6) |
| **الزبائن القديمة** | حالات المتابعة القانونية المرسلة لهذا المتابع، بلا الملغاة، والمكتملة في الأخير | `FollowerOldCustomersDataService`: `Status != Cancelled`، المكتملة أخيراً |
| **ملاحظات المندوبين** | `FollowerDelegateNotes` كتبها المتابع أو على قوائمه، قراءة فقط | `FollowerDelegateNotesDataService`: `CreatedByUserId == followerId` أو على قوائمه |

## الجباية

| الرقم | المصدر | الجديد |
|---|---|---|
| **القوائم:** مفعّلة أم لا، ووقت التفعيل. | `Delegates.FollowerCollectionEnabled/AssignedAtUtc` | `OrderList.FollowerCollectionEnabled/FollowerCollectionAssignedAtUtc` |
| **متابع يوم التفعيل** | مجموع `CustomersPaymentsRequest.Amount × 1448` في يوم التفعيل | مجموع `InstallmentPayments` التي أنشأها المتابع على القائمة في يوم التفعيل (بغداد)، **بالدينار بلا ×1448** |
| **مندوب أمس** | `View_CustomersPaymentsDelegate` لأمس | `InstallmentPayments` أمس التي أنشأها مندوب القائمة |
| **الدفعات** | TOP 400 × 1448 | `InstallmentPayments` على القوائم، مرقّمة، مع فلتر تاريخ واسم المحصّل |

## مواقع وصور المحلات

`GET shop-capture?branchId=&followerId=`. القاعدة في `FollowerWork/Common/ShopCaptureProgressQuery.cs`، ويستعملها أيضاً تطبيق المتابع (`GET app/Committee/list-cards`)، فالعدّادان متطابقان دائماً

- **النطاق:** كل قائمة لها متابع، والزبائن غير القانونيين بباقٍ مقرّب لأقرب 1000 أكبر من صفر (`ShopCaptureProgressQuery.HasRoundedOpenBalance`)، كما في المصدر (`FollowerShopCaptureOverviewService.cs:87-122` + `RatingDataSource.cs`).
- **«قائمة أرشيف» مستبعدة** (`ShopCaptureProgressQuery.ArchiveListName`): أنشأها سكربت ترحيل النجف لبقية المناطق، وليست في شاشة القديم. الاستبعاد هنا فقط، لا في التقييم ولا بقية الأقسام.
- **«له موقع»:** التقاط المتابع فقط: `CustomerLocation` بحالة `Captured` (المستورد من `CustomerShopFollowerCapture`، وما يسجّله تطبيق المتابع الآن). **لا يُحسب** GPS المحل الذي يكتبه موظف المبيعات ولا موقع الطلب.
- **«له صورة»:** آخر صورة من `CustomerShopPhotos` (مستوردة) أو `Attachments` بنوع `CustomerShopPhoto` (من تطبيق المتابع).
- **الواجهة:** نسب لكل قائمة. الجدول بفلاتر: الكل / بدون موقع / بدون صورة / مكتمل، مع بحث. الصورة تُفتح عبر `shop-capture/photos/{photoId}`.

## الموقع المباشر والمسارات

| القاعدة | المصدر | الجديد |
|---|---|---|
| **الحالة:** Live إذا ≤150 ثانية، وإلا Stale. بلا دوام = NoShift. بلا نقطة = NoLocation. | `SalesManagerContracts.cs:167-219` | `FollowerLocationRules.ResolveStatus` (نفس القاعدة، لا Offline) |
| **تاريخ المسار الافتراضي** | `toISOString()` (UTC) | تاريخ بغداد (`FollowerRoutesPage.vue`) |

## عدّادات التبويبات

`GET {followerId}/section-counts`، والملف `Queries/Admin/GetSectionCounts/GetFollowerSectionCountsQuery.cs`

- **«بانتظار»:** نفس قاعدة القائمة أعلاه.
- **بقية العدّادات:** `TotalCount` من نفس خدمات الأقسام بحجم صفحة 1، فالعدّاد يطابق القائمة دائماً.
- **`collectionEnabledLists`:** عدد قوائم المتابع المفعّلة للجباية.
