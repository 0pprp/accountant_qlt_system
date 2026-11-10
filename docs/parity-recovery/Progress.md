# Parity Recovery — Progress

تاريخ التحديث: 2026-11-10 (دفعات 05–09؛ بوابة قبول 10)  
النطاق: تصحيح مطابقة مستقلة 01–10 (لا إعادة بناء؛ لا إنتاج).  
المرجع القديم: `C:\GitHub\SalesHaider-main\SalesHaider-main` (قراءة فقط).

## حالة الدفعات

| دفعة | العنوان | الحالة | إثبات |
|---|---|---|---|
| 01 | جرد مسارات العمل | Implemented | جرد+وثائق |
| 02 | هوية الواجهة ومكونات العرض | Implemented | UI kit + evidence/02-ui |
| 03 | دورة طلب البيع | Implemented + hardened | Unit+API-live+Browser-live؛ Device جزئي |
| 04 | موظف المبيعات + تسلسل البيع | Implemented + hardened | أهلية موحّدة؛ ensure-draft ذري؛ A→B؛ شكوى+ملاحظة |
| 05 | مسؤول اللجنة / ميدانية + إحصائيات | Implemented | Unit+API-live؛ Device معلّق (لا adb) |
| 06 | مندوب جباية + مزامنة + تقييم أول | Implemented + payment-safety | Unit+API-live (idempotency/locks)؛ Device معلّق |
| 07 | لوحة الإدارة/المدير/المراقب + بروفايل | Implemented (harden) | Unit+API-live+Browser-live 360/768/1440؛ Device N/A ويب |
| 08 | مدخل إدارة الفروع + المدير المفوض | Implemented (visit-gate-hardened) | Unit+API-live+Browser-live+component Router/API؛ Device N/A ويب |
| 09 | لوحة المبيعات + محتوى مشترك + صلاحيات | Implemented (excel-search+url-harden) | Unit؛ API-live جزئي معلّق إن لم يُشغَّل iso؛ Browser/Device معلّق |
| 10 | قبول النظام / Go-NoGo | Acceptance package | evidence/10-acceptance — Device Missing حيث لا adb |

Implemented ≠ Unit ≠ API-live ≠ Browser-live ≠ Device.

## دفعة 05 — مسؤول اللجنة (Motaba)

### ميدانية (FieldFollowups) — شريحة عمودية كاملة
- مرجع: `FollowersController` FieldFollowups + `FollowerFieldFollowupService` (Add/List/Complete/record-complete/latest-notes).
- جديد: كيان `FollowerFieldFollowup` + CQRS تحت `Application/Committee/.../FieldFollowups` + مسارات:
  - `GET/POST /api/v1/app/committee/field-followups`
  - `POST .../field-followups/{id}/complete`
  - `POST .../field-followups/record-complete`
  - `GET .../field-followups/latest-notes?listId=`
- نطاق: كاتب = Motaba المكلّف على القائمة؛ ليس ManagementTask ولا LegalFollowup.
- Flutter: `FieldFollowupsPage` حقيقية + أزرار «إرسال للمتابعة الميدانية» / «تمت المتابعة» / «ملاحظة زبون» على كروت `ListDetailPage` بمكوّنات 02 و`centered_dialog`.
- Migration رسمي: `20261109230458_FollowerFieldFollowups` مطبّق على `qalaat_22_23_iso`.

### إحصائيات ListStats
- `GET /api/v1/app/committee/list-stats` (+ `/{listId}`) — معادلات مرجعية من Orders الحية + لقطات تقييم الزبون.
- **اختلاف مقصود موثّق:** `PreviousMonthAvailable=false` و`PreviousMonthFrozen=false` حتى تتوفر لقطات شهرية مجمّدة؛ لا صفر وهمي للشهر السابق.
- درج الإحصائيات في الدرج → `ListStatsPage` (لم يعد ComingSoon).

### ملاحظات (ثلاث مسارات منفصلة)
- CustomerNote: `POST .../customers/{id}/notes` + ظهور `HasNote` في daily.
- DelegateNote: زر على كارت القائمة (`DelegateNoteButton`).
- DirectorNote: ملاحظة للمدير (موجودة).
- Daily يعرض أيضاً `LatestFieldFollowupNote`.

### API-live 05 (`evidence/05-committee/api-live-05.log`)
```
LOGIN_MOTABA=OK
FIELD_ADD + IDEMPOTENT pending sameId=True
RECORD_COMPLETE=Completed
LATEST_NOTES=1
HASNOTE_AFTER=True
LIST_STATS PREV_AVAILABLE=False (مقصود)
```

### Unit 05
`FollowerFieldFollowupDomainTests` — Passed ضمن filter.

## دفعة 06 — مندوب / جباية / مزامنة

### ClientPaymentId (عقد جديد صريح — ليس مكتشفًا قديماً في API المركزي)
- عمود `InstallmentPayments.ClientPaymentId` + فهرس فريد جزئي.
- Migration: `20261109230700_InstallmentPaymentClientPaymentId` على iso.
- Create + Sync: نفس المفتاح = نجاح دون قبض ثانٍ؛ لا توليد مفتاح جديد بعد timeout.
- Flutter remote يرسل `clientPaymentId` في create وsync.

### جباية المتابع (قائمة فقط)
- السيرفر: Create وSync يرفضان المندوب عند `FollowerCollectionEnabled` برسالة `FollowerCollection.MandobLocked`.
- Profile يعرض `orderList.followerCollectionEnabled`.
- الواجهة: حوار قبل التحصيل يوضح القفل على القائمة دون حظر بقية الحساب.

### طابور الجهاز
- `listUploadable` / `reclaimForOwner` مشدّدان: لا عرض/استرداد طابور حساب آخر؛ logout يرفض مع وجود معلّق (سياسة استرداد صريحة عبر reclaim لنفس المندوب فقط).

### تقييم أول تحصيل / WrongSale
- مسار موجود: `POST /api/v1/app/WrongSales/first-collection-eval` + حوار التقييم قبل التحصيل للمبيعات الجديدة؛ فشل التقييم لا يُعرض نجاحًا ولا يُنشئ تسديدًا.

### API-live 06 (`evidence/06-delegate/api-live-06.log`)
```
PAY1=OK
PAY1_RETRY_SAME_KEY=OK_IDEMPOTENT
KEY_COUNT 1
PAY_WHILE_LOCKED=403 Forbidden
```

### Unit 06
`delegate_app` tests: 12 passed (PaymentCollectionRules + forms).

## سلامة التحصيل والمزامنة (قبل 07 — harden 06)

عقد موحّد عبر `AppInstallmentPaymentWriter` لـ Create و Sync:

1. **ClientPaymentId** مربوط بـ CreatedBy + OrderId + Amount + Date.
   - نفس المفتاح ونفس البصمة → `alreadyApplied=true` (إقرار دون قبض ثانٍ).
   - نفس المفتاح وpayload مختلف → `409 ClientPaymentPayloadConflict`.
   - حساب آخر → `OrderNotFound` بلا كشف `paymentId` (لا Any(ClientPaymentId) وحده).
2. إعادة إقرار بعد `Completed` مسموحة بعد مطابقة الهوية؛ دفعة جديدة على مكتمل مرفوضة.
3. Flutter: لا إعادة كتابة OrderId تلقائياً للمعلّق؛ عند غموض/فقدان وصول تُحفظ العملية مع سبب التعثر؛ الحذف المحلي فقط بعد إقرار يثبت نفس المفتاح/الطلب/المبلغ.
4. 403/400/نص خطأ ≠ نجاح مكرر. Create→Sync يستخدم نفس تاريخ التحصيل.
5. قفل ذري `pg_advisory_xact_lock(namespace, entityId)` — طلب=`791902` / نقد=`791903` (بعيد عن ensure-draft=`791901`؛ بلا GetHashCode). الترتيب: طلب ثم حامل نقد.
6. Flutter `parseServerAck`: ClientPaymentId+OrderId+Amount+Date + PaymentId>0 قبل الحذف المحلي.
7. إثبات عمليتي API (4200+4201): Create∥Sync عبر عمليتين؛ تجاوز متبقي؛ طلبان لنفس الحامل بلا فقد رصيد — `api-live-dual-process-locks.log`.

### API-live safety (`evidence/06-delegate/api-live-payment-safety.log`)
```
CONCURRENT Create+Sync KEY_COUNT=1
CONFLICT_AMOUNT_STATUS=409
OTHER_ACCOUNT_STATUS=400 leakPaymentId=False
ACK_AFTER_COMPLETE already=True؛ NEW_AFTER_COMPLETE=400
OVER_REMAINING okCount=1 errCount=1
TWO_CONCURRENT_CASH delta=2000 ok=True
RETRY_CREATE_THEN_SYNC ack=True sameId=True orderId=1
```

### Unit safety
`ClientPaymentIdSafetyTests` — Passed 6 (incl. lock namespaces). Flutter `payment_ack_parse_test` — Passed.


## دفعة 07 — لوحة الإدارة والمدير والمراقب

- ManagementSummary مستقل عن SalesSummary؛ DelegateFollowup (OrderList.Id، نشاط ≤40ي)؛ ManagementRatings (فئة ≤30ي)؛ FollowerWork 8 أقسام؛ FollowerDeclining=7ي ≠ BranchDeclining=15ي؛ RequiredDue يستثني IsLegal.
- المراقب: لا ManagementSummary افتراضياً (403)؛ منح Read يعمل عبر صلاحية فقط. المدير+المراقب نفس اللوحة.
- Harden: عودة بروفايل التدقيق تسمح بمسارات follower-work + route names؛ إصلاح `params.id` لزبائن متابعة المندوبين؛ رابط بروفايل من التقييمات؛ تنقّل أقسام المتابعين يحافظ على branchId/followerId.
- API-live (`evidence/07-management/api-live-07.log`): SUMMARY_MATRIX؛ MONITOR_SUMMARY=403؛ MONITOR_RATINGS=OK؛ BM_SUMMARY=403.
- Browser-live: summary-360/768/1440.png.

## دفعة 08 — إدارة الفروع والمدير المفوض

- أبوان مستقلان APP_TABS 10×10 تطابق home_shell المصدر؛ لا تكرار موديولات.
- بوابة زيارة مغلقة فعليًا لأعمال `/management/branch` فقط:
  - لا تركيب RouterView للأعمال ولا طلبات API أثناء loading/error/عدم استيفاء الزيارة؛ visit-workspace بلا حلقة توجيه.
  - الزيارة مربوطة بالفاعل + selectedBranch؛ زيارة A لا تفتح أعمال B؛ إعادة تقييم عند تبديل الفرع/فتح/مغادرة.
  - نقص BranchVisit.Read لا يمنح تجاوزًا. مستثنى: oversight + تقارير المفوض/تاريخ الزيارات.
  - حراسة سيرفر BranchVisitRequirement على أعمال الفرع (Customers/Payments/NewSales/Declining + قرارات/أسماء قانونية) — التحويل بالواجهة وحده لا يكفي.
- OpenBranchVisit: idempotent لنفس الفرع؛ فرع آخر → AlreadyOpen/400. BM seed يحصل Task.Read+إجراءات inbox.
- API-live (`evidence/08-branch-delegate/api-live-08.log`): OPEN2_IDEMPOTENT؛ OTHER_BRANCH=400؛ WrongSales/NewSales/Tasks OK.
- Browser-live: branch-visit-768.png؛ harden: `evidence/08-branch-delegate/VISIT_GATE_HARDEN.md` + `branchVisitGate.component.test.ts`.
- فجوات مفتوحة موثّقة: LegalConversion Decide UI؛ exception hold mark-read؛ committeeRatings not_implemented؛ لا Device لهذه الوحدات الويب.


## دفعة 09 — لوحة المبيعات والمحتوى والصلاحيات

- لوحة sale: overview/requests/employees/followup/live-map/routes/completed/summary/legal مربوطة مسبقاً؛ طلبات تحت أب المبيعات.
- هوية الموظف: SalesEmployeeAssignment (ليس Mandob وحده). قطع بيع 03:00 بغداد ≠ مسار يوم تقويمي.
- Realtime SalesRequestHub: مجموعات من هوية السيرفر؛ الواجهة تعرض connected/reconnecting + polling معلن.
- **جديد 09:** بحث Excel POST /admin/SalesExcelSearch/customers + /sale/excel-search (Customer.Read، نطاق الجلسة، بلا N+1 لكل اسم).
- مشتريات: مدخل finance /purchases موجود — ليس تحت sale (اختلاف مقصود موثّق).
- محتوى: PriceBoard إعلان أسعار؛ Announcements AppKey (Follower=Motaba)؛ CommitteeTasks ≠ ManagementTasks؛ AppNote ≠ Complaint.
- Tutorial: تشديد روابط https لمضيفي YouTube/Vimeo فقط.
- **تشديد جمهور التطبيق:** ContentAppAudience يربط appKey بهوية الجلسة (تكليف موظف المبيعات يسبق Mandob)؛ AudienceDenied على قائمة/وسائط عامة.
- فجوات P0 متبقية موثّقة: UI تكليف SalesEmployeeAssignment؛ إعلانات sales_employee_app seed.
- صلاحيات: PowersForm Inherit/Allow/Deny + baseline/expectedVersion؛ قوالب تشغيلية بلا ManagePermissions.
- دليل: evidence/09-sales-content/.

## دفعة 10 — قبول النظام (بوابة)

- حزمة Go/No-Go في evidence/10-acceptance/ — لا نشر إنتاج.
- Device Missing حيث لا adb؛ لا Flutter Web كإثبات Android/GPS.

## فجوات متبقية (تشمل نواقص 03–04)

| فجوة | أولوية | دفعة |
|---|---|---|
| existing-customer من المرجع القديم | P1 | 03 |
| Device screencap أسود / لا adb في هذه الجولة | Device جزئي | 03–06 |
| لقطات شهرية مجمّدة ListStats (PreviousMonth) | P1 موثّق | 05 |
| Browser-live 360/768/1440 لشاشة الإحصائيات/الميدانية | معلّق | 05 |
| إثبات Device لوحات مفاتيح 360/412 | معلّق (لا جهاز/adb) | 05–06 |

## أوامر مثبتة

```
dotnet ef database update → FollowerFieldFollowups + ClientPaymentId على qalaat_22_23_iso
dotnet test --filter FollowerFieldFollowup|WrongSale|Installment → Passed 13
flutter test delegate_app → 12 passed
API :4200 iso
  evidence/05-committee/api-live-05.log
  evidence/06-delegate/api-live-06.log
  evidence/06-delegate/api-live-payment-safety.log
dotnet test --filter ClientPaymentIdSafety → Passed
```

لا نشر إنتاج.

## تحقق مفاتيح الأقفال والإقرار — 2026-11-10

تمت مراجعة التعديلات الموجودة دون إعادة بنائها: namespaces ثنائية ثابتة order=791902 وcash=791903، منفصلة عن ensure-draft=791901 والقفل أحادي المفتاح. لا GetHashCode في كاتب التحصيل؛ ترتيب الطلب ثم حامل النقد محفوظ.

أُعيد الإثبات عبر عمليتي API مستقلتين من publish محلي على 4202/4203 ضد qalaat_22_23_iso فقط. الاختبار الجديد يمسك القفل مؤقتًا ويتأكد عبر pg_locks أن جلستي العمليتين تنتظران نفس القفل قبل تحريره؛ بالتالي Create وSync متداخلان فعلًا، وليس إرسالًا متتابعًا كما في أول سيناريو السكربت السابق.

- concurrent Create/Sync: نفس PaymentId، KEY_COUNT=1، إقرار واحد alreadyApplied.
- مفتاحان يتجاوزان المتبقي: 200 + 400، زيادة المقبوض 1000 فقط.
- طلبان مختلفان لحامل نقد واحد: كلاهما 200، cash delta=2000.
- دليل آلي مع assertions: evidence/06-delegate/dual-process-lock-barrier.json.
- سكربت إعادة الإنتاج: evidence/06-delegate/verify-payment-lock-barrier.py. يتطلب متغيرات PAYMENT_VERIFY_DSN/USER/PASSWORD/OUTPUT؛ يتحقق من اسم قاعدة iso والعنوان الحلقي، ولا يطبع tokens. يستخدم ويعدّل fixture الطلبين 1 و3 في iso؛ لا يُشغّل على الإنتاج.

Flutter: الإقرار يشترط المفتاح والطلب والمبلغ والتاريخ بصيغة DateOnly المطابقة وPaymentId موجبًا صحيحًا. أُضيف رفض المعرف الكسري والتاريخ الذي يحمل لاحقة timestamp بدل عقد DateOnly؛ لا تحويل إلى رقم صحيح يخفي بيانات غير صالحة. الرد غير الناجح أو الناقص لا يسمح بحذف الطابور.

النتائج: ClientPaymentIdSafetyTests 6/6؛ Flutter delegate full suite 24/24 (اختبارات الإقرار 12 حالة). publish API ناجح مع تحذيرات موجودة في ملفات أخرى. evidence/06-delegate/flutter-lock-ack-tests.log يحفظ النتيجة. لا تعديل مخطط، لا إنتاج، لا تغيير تصميم، ولا ادعاء Device جديد.
