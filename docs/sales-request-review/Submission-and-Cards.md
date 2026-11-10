# إدخال طلبات البيع وتصميم الكارتات

تحديث 2026-11-06. هذا الملحق يصف تنفيذ الإرسال من موظف المبيعات والمندوب ومسؤول اللجنة، ويحدّث حالة ingestion السابقة لهذه المصادر الثلاثة فقط. Excel/CRM والقانونية خارج هذا التغيير.

أربعة حقول إلزامية: اسم الزبون، الهاتف الذي يبدأ 07 وطوله 11، العنوان، المنتج المطلوب. الملاحظة اختيارية. الفرع يختار من الفروع المصرح بها عند تعددها.

كل إرسال ينشئ SalesRequest واحدًا بحالة New وبلا موظف مستهدف أو Customer أو Order جديد. يبقى في غير المسند للمطابقة والإسناد اليدوي. سجل الحركة وoutbox يُحفظان في نفس المعاملة. القفل في PostgreSQL يخص نفس المستخدم ومفتاح الإرسال فقط؛ الطلبات المختلفة لا تشترك بالقفل. المفتاح نفسه مع محتوى مختلف يرجع 409.

الصلاحيات SubmitEmployee وSubmitDelegate وSubmitFollower خارج Permissions.All. دور Mandob الحالي مستخدم للمبيعات والمندوب ولذلك مُنح أول اثنتين افتراضيًا، وMotaba الثالثة؛ يمكن تعديل التفويض من محرر الصلاحيات. مصدر التطبيق سياق مصرح به وليس إثبات هوية حزمة التطبيق.

الكارت يعرض الاسم والهاتف والمنتج والعنوان والمُرسل والمصدر والموظف والفرع والمحافظة والملاحظات والأسباب والتواريخ والمرجع والبيع المرتبط عند وجودها. لا قص للملاحظات. التفاصيل والمطابقة والأكشنات باقية. السجلات القديمة ذات الحقول الناقصة تعرض ذلك صراحة.

نجحت اختبارات API الحية للمصادر الثلاثة، وإعادة الإرسال والتزامن والأخطاء، وعرض أربعة معايير. اختبار Flutter هو Widget باعتراض نقل معزول، وليس إثبات جهاز. تجربة Vue من متصفح فعلي. الإنتاج لم يُمس، ولا migration مولدة. SQL اليدوي لا يسوي ModelSnapshot للإنتاج.

## Endpoints

| Method | Path | Permission | Purpose |
|---|---|---|---|
| GET | /api/v1/app/SalesRequests/submission-branches/{source} | SubmitEmployee / SubmitDelegate / SubmitFollower | Authorized branches for the selected source |
| POST | /api/v1/app/SalesRequests/submit/{source} | Source-specific permission + branch membership | One unassigned SalesRequest with timeline and outbox |
| GET | /api/v1/admin/SalesRequests/filter-options?tab=&branchId= | Same tab and branch scope as request list | Employee and province names for filters |

## Fields

| Field | Required | Validation | Storage |
|---|---|---|---|
| customerName | Yes | Trimmed; 1–120 characters | SalesRequests.CustomerName |
| customerPhone | Yes | 07 + 9 digits; Arabic/Persian digits normalized | SalesRequests.CustomerPhone |
| address | Yes | Trimmed; 1–500 characters | SalesRequests.Address |
| requestedProduct | Yes | Trimmed; 1–200 characters; free text | SalesRequests.RequestedProduct |
| note | No | Up to 1000 characters; blank becomes null | SalesRequests.SubmissionNote |
| branchId | Yes | Current user DB branch membership | SalesRequests.BranchId |
| submissionKey | Yes | UUID; unique per submitting user | SalesRequests.SubmissionKey |

## Acceptance

| Area | Status | Evidence | Limit |
|---|---|---|---|
| Backend build | Passed | Build succeeded after changes | Existing nullable warnings remain |
| SalesRequests tests | Passed | 94/94, including 13 intake checks | Unit/relational suite; not mobile evidence |
| Three sources API | Live verified | New + unassigned, fields, four matching cards | Isolated qalaat_04f_sales_verify only |
| Retry and concurrency | Live verified | Four simultaneous retries return one request ID | Operation keyed to user and UUID |
| Authorization | Live verified | 401 anonymous; 403 unsupported source/branch | Source is a permissioned context, not app attestation |
| Flutter forms | Widget tested | 3 tests per app; submit/retry/phone | No physical Android device verified |
| Vue workspace | Browser verified | Desktop/mobile screenshots; no page errors or horizontal overflow | Existing old test records may have missing fields |
| Vue module tests | Passed | 14/14; production build passed | Global type-check still has 8 unrelated errors |
| Database | Local only | SCHEMA_Submission.sql applied to isolated DB | No migration; snapshot not reconciled for production |

## Files

| Layer | Path | Responsibility | Notes |
|---|---|---|---|
| Domain | api-main/api-main/src/Domain/Entities/SalesRequestAggregate/SalesRequest.cs | Submission fields and initial unassigned state | Does not create Customer or Order |
| Application | api-main/api-main/src/Application/SalesRequests/Commands/App/Submit/ | Validation, permissions, branch check, fingerprint, receipt | User identity from session |
| Infrastructure | api-main/api-main/src/Infrastructure/Services/SalesRequestSubmissionLock.cs | PostgreSQL transaction lock for one user operation | Unique index remains a second safeguard |
| API | api-main/api-main/src/Api/Controllers/V1/AppArea/SalesRequestsController.cs | Authenticated submission and branch endpoints | Source-specific permission enforced by handler |
| Vue | panel-admin-main/panel-admin-main/src/modules/SalesRequests/components/ | Readable cards, details, named filters and matching cards | Existing actions and permission gates retained |
| Flutter | Each app: lib/features/sales/presentation/new_sales_request_page.dart | Four required fields, optional note, branch selection and retry | Source fixed by app wiring |
| Flutter | Each app: lib/features/sales/data/sales_request_submission.dart | Existing authenticated Dio client and intake contract | No legacy service calls |
| SQL | docs/sales-request-review/SCHEMA_Submission.sql | Nullable fields for legacy rows and unique submission key | Execution guard restricts the local verification DB |
