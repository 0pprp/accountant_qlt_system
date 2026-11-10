# CardParityMatrix — جرد معلومات كارت/تفاصيل طلب البيع

**حدود الدليل:** لا يوجد مصدر واجهة SalesHaider للكارت داخل هذا المستودع. خلايا «القديم» = ما وُثّق سلوكيًا في `design-and-rules.md` / `Sources-04E.md` فقط، وإلا `غير متاح في المستودع`. لا حقول مخترعة.

## مكوّنات الجديد المسؤولة

| سطح | مسار |
|-----|------|
| كارت اللوحة | `panel-admin-main/.../SalesRequests/components/RequestCard.vue` |
| قائمة الكروت | `.../RequestList.vue` |
| التفاصيل | `.../RequestDetailsPanel.vue` + `RequestLifecycleActions.vue` |
| مساحة العمل | `.../views/SalesRequestsWorkspace.vue` |
| كارت الموظف | `sales_employee_app/.../sales_requests_bin_page.dart` (`ListTile`) |
| تفاصيل الموظف | `.../sales_request_detail_page.dart` |
| عقد الموظف | `.../domain/entities/sales_request_item.dart` |
| عقد الأدمن | `AdminSalesRequestListItem` + `AdminSalesRequestRowMapper.cs` |
| عقد التطبيق | `SalesRequestAppItemDto` (`SalesRequestAppDtos.cs`) |

## الحقول

| المعلومة | مصدر القديم | مصدر API الجديد | مكان الواجهة الجديدة | الحالة | شروط الظهور |
|----------|-------------|-----------------|----------------------|--------|-------------|
| اسم الزبون | غير متاح في المستودع (وثّق وجود كروت قديمة) | Admin `CustomerName` / App `CustomerName` | كارت+تفاصيل اللوحة؛ عنوان كارت/تفاصيل Flutter | موجود | دائمًا |
| رقم الطلب | غير متاح | Admin `Id` / App `RequestId` | `#id` باللوحة؛ subtitle Flutter | موجود | دائمًا |
| الحالة / التسمية | لوحات حالة متعددة (موثّق) | `Status` + `StatusLabel` + `TabKey` | كارت/تفاصيل اللوحة وFlutter | موجود | تبويب اللوحة بصلاحية Tab* |
| الهاتف | غير متاح | `CustomerPhone` | كارت/تفاصيل اللوحة؛ تفاصيل Flutter | موجود | `—` إن null باللوحة |
| الموظف المستهدف | غير متاح | Admin `TargetEmployeeId`+`TargetEmployeeName`؛ App `TargetEmployeeId` فقط | كارت/تفاصيل اللوحة فقط | موجود باللوحة / ناقص بالـFlutter (لا اسم) | `—` إن غير مسند |
| المصدر / تسمية المصدر | غير متاح | Admin+App `SourceType`+`SourceTypeLabel` | كارت/تفاصيل اللوحة؛ فلتر المصدر | موجود باللوحة / غير مربوط بـFlutter (API يرجعه والنموذج لا يعرضه) | فلتر؛ `IsSentSource` لتبويب المرسلة |
| OriginalExternalId | غير متاح | Admin `OriginalExternalId` | لا عرض | غير مربوط | — |
| الفرع | غير متاح | `BranchId`+`BranchName` | كارت/تفاصيل اللوحة؛ تفاصيل Flutter | موجود | fallback لـ Id |
| المحافظة | غير متاح | Admin `ProvinceId`+`ProvinceName` | سطر الفرع بالكارت + فلتر | موجود جزئيًا (كارت/فلتر؛ ليس سطر تفصيل مستقل) | إن وُجدت |
| وقت الإضافة | غير متاح | `CreatedAt` | كارت/تفاصيل اللوحة؛ subtitle Flutter فقط | موجود / ناقص في شاشة تفاصيل Flutter | — |
| آخر تغيير حالة | غير متاح | `StatusChangedAtUtc` | كارت/تفاصيل اللوحة وFlutter | موجود | `—` إن null |
| مدة البقاء بالحالة | غير متاح | مشتق بالعميل من التواريخ | كارت اللوحة فقط | موجود | — |
| ملاحظة الحالة | غير متاح | `LastStatusNote` | تفاصيل اللوحة؛ Flutter subtitle/detail | موجود | إن غير فارغة |
| سبب الرفض | غير متاح | Admin `RejectReason` | تفاصيل اللوحة | موجود باللوحة / ناقص بـFlutter | عند الرفض/استثناء معروض |
| اسم المُنشئ | غير متاح | Admin `CreatedByName` | لا عرض (مفتاح ترجمة موجود بلا ربط) | غير مربوط | — |
| البيع المرتبط | غير متاح | `LinkedOrderId` | رابط بالتفاصيل؛ Flutter عند الربط | موجود | إن LinkedOrderId>0؛ اللوحة تحتاج صلاحية Order |
| زبون موجود / بروفايل | بروفايل مكدس (موثّق) | Admin `ExistingCustomerId` | رابط بروفايل باللوحة | موجود | إن ID + صلاحية ReadAuditProfile/Customer.Read |
| IsDeleted / DeleteReason / DeletedAt | غير متاح | حقول Admin | تفاصيل + lifecycle | موجود باللوحة | تبويب المحذوفة / بعد الحذف |
| حالة الاستثناء / قرار / استهلاك | بوابات قديمة موثّقة | `ExceptionState`+أوقات | تفاصيل + lifecycle | موجود باللوحة | تبويب الاستثناءات؛ أزرار القرار عند pending + ExceptionDecide |
| ViewedAtUtc | غير متاح | App `ViewedAtUtc` | لا عرض (يُستدعى view) | غير مربوط بصريًا | بعد فتح التفاصيل |
| IsReturned | توافق مفاتيح قديمة في TabMap | App `IsReturned` | Flutter chip/subtitle | موجود بالموظف | عند Returned |
| مبالغ مالية على الكارت | غير متاح كحقل كارت طلب | لا على SalesRequest list DTO | لا على كارت الطلب | غير موجود في عقد الطلب (المبالغ على Order/البيع) | — |
| بطاقات المطابقة | evaluate/hits قديمة موثّقة | MatchRead endpoints | مساحة تدقيق غير المسند | موجود | unassigned + MatchRead |
| الطلبات المكررة | موثّق | duplicate-requests | مساحة تدقيق | موجود | نفس سياق التدقيق |

## الإجراءات

| الإجراء | القديم | API الجديد | الواجهة | الحالة / الظهور |
|---------|--------|------------|---------|-----------------|
| فتح/اختيار | غير متاح | list / get-by-id | كارت | موجود |
| إسناد/إعادة إسناد | يدوي/CRM متفاوت (موثّق) | `Assign` | لوحة | موجود؛ يحجب بـ exception hold |
| رفض | غير متاح | Admin Reject / AppReject | لوحة + Flutter | موجود؛ ملاحظة مطلوبة بالموظف |
| طلب استثناء | غير متاح | RequestException | لوحة | موجود بالصلاحية |
| قرار استثناء | غير متاح | ExceptionDecide | lifecycle | موجود عند pending فقط |
| حذف ناعم / استعادة | غير متاح | SoftDelete / Restore | lifecycle | موجود؛ قيود البيع المرتبط/المكتمل |
| تجهيز / تعليق / كشف | مفاتيح قديمة mapped | AppPrepare/Pend/Inspect | Flutter تفاصيل | موجود حسب Transition Matrix |
| ensure-draft / بدء بيع | غير متاح | AppLinkDraft | Flutter | موجود؛ يحتاج زبون مرتبط |
| القانونية | — | Tab Not Implemented | زر معطل | Not implemented |
| ingestion المرسلة | CRM/يدوي قديم | فلتر فقط | تبويب مرسلة | Not implemented (ingestion) |

## فجوات بارزة للتصميم التالي (بدون إسقاط معلومات الآن)

1. Flutter لا يعرض المصدر رغم وجوده في API.
2. Flutter لا يعرض اسم الموظف / سبب الرفض / ViewedAt.
3. CreatedByName غير مربوط باللوحة رغم وجوده في DTO.
4. OriginalExternalId غير معروض.
5. المبالغ ليست جزءًا من كارت SalesRequest (تبقى على Order) — لا تُختلق هنا.

## لقطات 04H (بدون قص معلومات)

| ملف | العرض | المحتوى |
|-----|-------|---------|
|  4h-live-evidence/screenshots/panel_card_detail_1440.png | 1440 | كارت #8 + تفاصيل + سجل حركة (اسم طويل، Employee B، ملاحظة 04H B prepare، بيع مرتبط #6، فرع ناقص ???) |
|  4h-live-evidence/screenshots/panel_card_detail_360.png | 360 | نفس التفاصيل على عرض ضيق |
|  4h-live-evidence/screenshots/panel_card_list_1440.png | 1440 | قائمة الكارت قبل/مع البحث |
| Flutter 18_home_bins_after_dio_fix.png وغيرها | 390 | Web login/duty/home — صناديق الحالات فارغة بسبب Dio unknown |
