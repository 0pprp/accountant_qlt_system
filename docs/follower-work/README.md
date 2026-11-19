# عمل المتابعين — المرجع

نقل شاشة «عمل المتابعين» من SalesHaider (`origin/main`) إلى هذا النظام، بنفس تدفق البيانات، مع UX مختصر وتصميم neumorphic موحّد مع «متابعة المندوبين».

## الملفات

| الملف | المحتوى |
|---|---|
| [rules.md](rules.md) | قاعدة كل كارت ورقم، مع مرجع المصدر ومقابله هنا |
| [data-flow.md](data-flow.md) | من يكتب البيانات (التطبيقات) ← الجداول ← الـ endpoints ← الشاشة |
| [security.md](security.md) | الصلاحيات، عزل الفروع، القيود على الكتابة، التدقيق |
| [differences.md](differences.md) | كل فرق عن القديم وسببه (عيوب المصدر الـ14 وما أُصلح منها) |

## شكل الشاشة

صفحة واحدة: `/follower-work?branchId=&followerId=&tab=&customerId=`

- **العمود الجانبي:** المتابعون في الفرع، مع التقييم الوهمي وسهم الاتجاه وعدد القوائم وعدد زبائن التقييم، وبحث مدمج. على الجوال يتحول إلى قائمة منسدلة.
- **شريط التبويبات:** فوق المتابع المختار. لكل تبويب عدّاد من `section-counts`. يظهر التبويب فقط لمن يملك صلاحيته.
- **اللوح الجانبي للزبون:** يفتح عند الضغط على أي زبون، ويعرض الصناديق الأربعة دون مغادرة الصفحة.
- **حفظ الحالة:** كل الحالة في الرابط، فالرجوع من «البروفايل الكامل» يعيد نفس المتابع والتبويب والزبون.
- **السايدبار:** عناصره التسعة تفتح نفس الصفحة على التبويب المطلوب. الروابط القديمة `/management/follower-work/*` تعيد التوجيه إليها.

## أين أعدّل؟

| أريد تغيير… | الملف |
|---|---|
| عتبات الفئات أو التاريخ الوهمي | `api-main/api-main/src/Application/SalesRatings/Common/CustomerRatingCalculator.cs`، `PhantomSaleDate`، `ListRatingAggregator` |
| من يدخل مجموعة التقييم (cohort) | `Application/SalesRatings/Common/ActiveRatingCohort.cs`، `ActiveCustomerRules.cs` |
| حقائق الزبون حتى تاريخ معيّن (أمس، قبل أسبوع) | `Application/FollowerWork/Common/FollowerFactsQuery.cs` |
| كروت المتابع والتقييم الوهمي | `Application/FollowerWork/Common/FollowerWorkDataService.cs` |
| كروت مصادقة البيع والأسماء | `Application/FollowerWork/Common/FollowerSalesReviewDataService.cs`، `FollowerSaleAuthRules.cs` |
| قاعدة المتدنية | `Application/FollowerWork/Common/FollowerDeclineRules.cs`، `FollowerDecliningDataService.cs` |
| الملاحظات / ملاحظات المندوبين / القدامى | `FollowerNotesDataService.cs`، `FollowerDelegateNotesDataService.cs`، `FollowerOldCustomersDataService.cs` |
| الجباية وتفعيلها | `FollowerCollectionDataService.cs`، `Api/Controllers/V1/AdminArea/OrderListsController.cs` (`PATCH {id}/follower-collection`) |
| قفل دخول المندوب أثناء الجباية | `Application/Users/Commands/Common/Login/LoginHandler.cs` |
| مواقع وصور المحلات | `Application/FollowerWork/Queries/Admin/GetShopCapture/*`، `GetShopPhoto/*` |
| الصناديق الأربعة للزبون | `Application/FollowerWork/Queries/Admin/GetCustomerProfile/*` |
| إنشاء/تعديل/حذف متابع | `Application/FollowerWork/Commands/Admin/{Create,Update,Delete}Follower/*`، `ManageFollower/FollowerAccountRules.cs` |
| عدّادات التبويبات | `Application/FollowerWork/Queries/Admin/GetSectionCounts/GetFollowerSectionCountsQuery.cs` |
| الموقع المباشر والمسارات | `FollowerLocationRules.cs`، `FollowerTrackingDataService.cs`، `Api/Controllers/V1/AdminArea/FollowerTrackingController.cs` |
| الصلاحيات والتبويبات | `Application/Common/SeedData/Permissions.cs` (`FollowerWork`)، `Application/Common/AdminPanel/AdminPanelTabs.cs`، `RoleDefaultMatrix.cs` |
| الصفحة | `panel-admin-main/panel-admin-main/src/modules/FollowerWork/views/FollowerWorkBoard.vue` |
| اللوح الجانبي / نافذة الحساب / مواقع المحلات | `src/modules/FollowerWork/components/board/*` |
| أنماط الصفحة | `components/board/follower-work-board.css` (تلبس أقسام `fw-`/`fsr-`/`fdc-` المضمّنة) |
| رموز التصميم المشتركة | `src/modules/Core/components/neumorphic/neumorphic.css`، `NeuDialog.vue`، `NeuHistoryDialog.vue` |
| السايدبار والمسار | `src/modules/Core/navigation/navigation.registry.ts`، `Core/access/panelTabAccess.ts`، `src/modules/Management/router.ts` |

## الاختبارات

- `api-main/api-main/tests/Application.Tests/FollowerWork/FollowerWorkBoardParityTests.cs` يغطي:
  - تجاهل التسديدات والمبيعات بعد تاريخ المقارنة.
  - تجاهل القوائم الفارغة في متوسط المتابع.
  - رفض بروفايل زبون خارج قوائم المتابع أو خارج فرع المستخدم.
  - قفل دخول المندوب للتطبيق فقط ما دامت الجباية مفعّلة.
- بقية اختبارات `FollowerWork` و`DelegateFollowup` في نفس المجلد.
