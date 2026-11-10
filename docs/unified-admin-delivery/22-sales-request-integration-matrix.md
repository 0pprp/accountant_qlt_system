# 22 — مصفوفة تكامل طلبات البيع

| التطبيق/الشاشة | الزر | الحقول | API | الصلاحية | انتقال الحالة | أثر اللوحة | التنفيذ | دليل |
|---|---|---|---|---|---|---|---|---|
| موظف المبيعات — الرئيسية | طلب مبيع جديد | اسم، هاتف 07×11، عنوان، منتج نص، ملاحظة؟ | `POST /app/SalesRequests/submit/EmployeeSubmitted` + `submission-branches` | `SalesRequest.SubmitEmployee` | New / غير مسند | يظهر في Unassigned ضمن BranchId | Implemented | Unit phone+idempotency؛ Flutter form test |
| المندوب — الرئيسية | طلب مبيع جديد | نفسها | `…/submit/Delegate` | `SubmitDelegate` | New | نفس | Implemented | نفس |
| اللجنة/المتابع — درج → طلبات البيع | نموذج الإرسال | نفسها؛ Source=Follower من المسار | `…/submit/Follower` | `SubmitFollower` | New | نفس | Implemented | نفس |
| السيرفر Submit | — | Source من route؛ CreatedBy من الجلسة؛ Branch ضمن UserBranches | Handler + SubmissionKey lock | حسب المصدر | لا Order / لا إسناد تلقائي | Outbox Invalidation | Implemented | `SalesRequestSubmissionTests` |
| اللوحة — غير المسند | كارت الطلب | مطابقة 4 بطاقات | `GET …/match-hits` | `MatchRead` ⊥ قراءة التبويب | — | loading/failed/insufficientData/results | Implemented (دفعة سابقة) | FE MatchingSummary |
| اللوحة — بروفايل زبون | فتح من المطابقة | — | Customer profile | ReadAuditProfile | — | عودة بفلاتر الطلب | Implemented | مسار SalesRequests |
| موظف — دورة ميدانية | إعداد/تعليق/رفض/كشف/إتمام | ملاحظات حسب الإجراء | App transitions + ensure-draft | App* منفصلة | Incoming→…→Sold | عدّادات+سجل | Implemented | مسار موجود |
| إعادة إسناد A→B | إسناد إداري | TargetEmployee | Admin Assign | Assign | الحراسة على الملكية | A يفقد القراءة | Implemented | سياسة حالية |
| القانونية ميداني — متابع | تثبيت موقع / رفع / إكمال | GPS + 4 مستمسكات | `app/committee/legal-followups/*` | FieldRead/Upload/Complete | InProgress→Completed | انعكاس الحالة؛ معاينة لوحة | Implemented | Unit completion rules |
| اللوحة — قانونية | معاينة/تنزيل مستمسك | — | `GET …/documents/{id}/file` | LegalFollowup.Read + نطاق فرع | — | blob معاينة | Implemented | Endpoint + SalesLegalPage |
| Realtime Vue | — | Hub + polling معلن | SalesRequestHub | جلسة | — | متصل فقط عند Connected | Implemented | `salesRequestRealtime.ts` |
| Realtime Flutter موظف | — | Hub + reconnect | نفس | جلسة | — | لا «متصل» زائف | Implemented | `sales_request_realtime_client.dart` |

## عقد المنتج
- الحقل `RequestedProduct` نص (string) في SubmitBody — ليس ProductId.
- الواجهة تعرض «المنتج المطلوب» كنص حر؛ لا استبدال صامت بـ ProductId.

## Idempotency
- `SubmissionKey` (Guid) + قفل لكل مستخدم + بصمة محتوى؛ إعادة نفس المفتاح بنفس المحتوى تعيد Receipt دون صف جديد؛ اختلاف المحتوى → تعارض.

## نواقص حقيقية
| نقص | الحالة |
|---|---|
| Device-verified على أجهزة فعلية للتطبيقات الثلاثة | Missing إن لم يتوفر جهاز في جلسة التسليم |
| Browser-live لزر الإنشاء من التطبيقات (Web) | يُثبت عند توفر جلسة |
| إثبات دورة كاملة إسناد→إتمام على iso مع حسابات حية | API-live مطلوب على `qalaat_22_23_iso` |
| نواقص 10–19 الحية الموثقة سابقًا | Open — لم تُغلق بلا دليل |
