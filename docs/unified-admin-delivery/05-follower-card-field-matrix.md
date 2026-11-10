# 05 — مصفوفة حقول بطاقة المتابع

مرجع المصدر: `DirectorFollowerOpsService.GetFollowerCardsAsync` + `DirectorFollowerOpsFollowerCardDto` + بطاقات Vue.

## حقول الهوية والتكليف

| الحقل | المصدر القديم | الجديد | الحالة | ملاحظات |
|---|---|---|---|---|
| followerId | UserID | User.Id | منفّذ | مفتاح البطاقة |
| followerName | UserName | FullName / UserName | منفّذ | العنصر الأساسي في البطاقة |
| branchId / branchName | مدينة / قوائم | Branch من OrderLists المسندة | منفّذ | قد تتعدد الفروع |
| assignedListCount | عدد قوائم FollowerUserLists النشطة | عدد OrderList حيث MotabaId | منفّذ | تكليف حالي |
| assignedListIds | — | اختياري ملخص | منفّذ جزئيًا | لا تحميل كامل للقوائم في البطاقة |
| isActiveAssignment | UserState + قوائم | له قوائم في النطاق | منفّذ | بدون شارة «متصل» |
| hasActiveShift / online | Directory (غالبًا false) | — | **محذوف** | لا مصدر حقيقي |
| lastLogin | — | — | **محظور** | لا بديل لنشاط ميداني |

## مؤشرات البطاقة (منفّذة إن توفرت الصلاحية)

| الحقل | المصدر | الصلاحية | حالة 05 |
|---|---|---|---|
| phantomRatingToday | متوسط وهمي اليوم (قوائم المتابع) | FollowerWork.Read | منفّذ |
| phantomRatingWeekAgo | متوسط قبل 7 أيام | FollowerWork.Read | منفّذ |
| scoreTrend | up/down/flat | FollowerWork.Read | منفّذ |
| ratedCustomerCountToday | زبائن مقيّمون اليوم | FollowerWork.Read | منفّذ |
| ratedCustomerCountWeekAgo | قبل 7 أيام | FollowerWork.Read | منفّذ |
| countTrend | up/down/flat | FollowerWork.Read | منفّذ |
| asOfLocalDate | Asia/Baghdad | FollowerWork.Read | منفّذ |

## عدادات الأقسام الثمانية

| عداد | القسم | حالة 05 |
|---|---|---|
| pendingSales / declining / notes / oldCustomers / delegateNotes / collection / liveMap / routes | 06–13 | **غير مرسلة** — لا صفر وهمي؛ أقسام `not_implemented` |

قاعدة: إن فشل مصدر مؤشر التقييم لا تُسقط البطاقة؛ تُرجع الهوية مع `metricsAvailability=unavailable` لذلك المؤشر.

## عقد API

`GET /api/v1/admin/FollowerWork/followers`

Query: `branchId?`, `search?`, `page`, `pageSize`, `sort` (allowlist: `name`, `rating`, `lists`)

Response: `{ items[], page, pageSize, totalCount, totalPages }`  
Item: هوية + فروع التكليف + عداد قوائم + كتلة تقييم متاحة/غير متاحة — بلا حقول أقسام.

تفويض: `FollowerWork.Read` **أو** أي صلاحية قسم منفّذ يحتاج الدليل (مسجّلة للمستقبل؛ لا تفتح بيانات القسم).
