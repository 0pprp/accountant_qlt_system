# Coverage_31 — دمج وظائف الفرع والمفوض دون تكرار

تاريخ: 2026-11-09 (إغلاق فجوات متبقية + دليل حي)  
المصفوفة التفصيلية: `31-branch-delegate-final-parity.md`  
لا بدء دفعة 32. لا نشر.

## الحكم الإجمالي

| تطبيق | Implemented | Partial | Missing |
|---|---|---|---|
| فرع (10) | 10 | 0 | 0 |
| مفوض (10) | 10 | 0 | 0 |

**31 مغلقة وظيفيًا على iso** مع API-live + Browser-live (حساب مخوّل). Device-verified لزر Flutter مندوب: **غير متوفر جهاز** — الربط الكودي موجود؛ ليس Device-verified.

## ما نُفّذ في إغلاق الفجوات

| بند | الحكم | الدليل |
|---|---|---|
| Members + AssignedTasks عند Open | **Implemented** | لقطة BM؛ مهام حية؛ idempotent؛ live-api + unit |
| مرفقات المغادرة | **Implemented** | تحقق محتوى؛ فشل ملف لا يغادر؛ idempotent re-depart؛ تنزيل مخوّل فقط |
| WrongSale مندوب أول تحصيل | **Implemented** | `POST app/WrongSales/first-collection-eval`؛ ≤6 ينشئ Notice؛ ≥7 ينجح بلا Notice؛ زر Flutter مربوط في collect |
| زبائن/تسديدات فرع | **Implemented** | BranchCustomers بحث≥2؛ BranchPayments list-payments-sum قراءة فقط |
| تقييمات ≡ ManagementRatings | **Implemented** | مقارنة في 31-parity؛ trend endValue=10 ممتاز من بيانات معروفة |
| NewSales سقف 200 | **Implemented** | حد مصدر بلا paging؛ فتح تفاصيل → mark-read؛ حالة Sold لا تتغير |
| API-live iso | **PASS** | `evidence/31-branch-delegate/live-api.json` + supplement |
| Browser-live | **PASS** | لوحة على :8080 ضد API :15645؛ زبائن/تسديدات/مبيعات/خاطئة/تقييم؛ 360/768/1440 |
| Device Flutter زر | **Missing** | لا جهاز؛ كود مربوط فقط |

## عقود API

| Endpoint | صلاحية |
|---|---|
| BranchVisits open/rate/depart/attachments | BranchVisit.* |
| BranchCustomers/search | Customer.Read |
| BranchNewSales (+ mark-read) | BranchNewSale.* |
| WrongSales admin + app/first-collection-eval | WrongSale.* / InstallmentPayment.Create |
| ManagementRatings catalog/trend/list-payments-sum | ManagementRatings.* |

هجرات iso: `Branch31ComplaintsVisitsAndOrderRead` + `Branch31VisitMembersAttachmentsAndWrongSaleSource`.

## تحقق

| طبقة | حالة |
|---|---|
| Unit Batch31+WrongSale | 16 passed |
| vue-tsc | 0 |
| vite build | ✓ built in 8.59s |
| Migration iso | Applied |
| API-live | PASS على qalaat_22_23_iso |
| Browser-live | PASS (admin)؛ follower API 403 للمرفقات/WrongSales |
| Device | Missing — لا جهاز |

## نواقص سابقة (10–30)

تبقى كما في `32-release-readiness-backlog.md` — لم تُسقط.
