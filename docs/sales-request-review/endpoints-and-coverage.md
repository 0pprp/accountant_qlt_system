# تدقيق طلبات البيع — التبويبات / الصلاحيات / Endpoints / الاختبارات

حالات الحالة المستخدمة: **Implemented** / **Tested** / **Not implemented** / **Schema-blocked**.

## الحالة الإجمالية

| المكوّن | الحالة |
|---------|--------|
| قواعد المطابقة الأربع (Domain) | Implemented + Tested |
| كيان SalesRequest + IsUnassigned + parity | Implemented + Tested |
| DbSet + Permissions خارج All + Admin seed | Implemented |
| جدول DB / ModelSnapshot | Schema-blocked (SQL يدوي) |
| Endpoints غير المسند / summary / hits / duplicates | Implemented — تشغيل حي Schema-blocked |
| Assign / Reject / RequestException + concurrency | Implemented (+ unit stamp) |
| CRM auto-assign | Not implemented (خارج المرحلة) |
| UpdateRequest | Not implemented (لا endpoint/CTA) |
| واجهة Unassigned + بطاقات + أكشنات | Implemented (mock عند فشل API فقط) |
| بروفايل تدقيق overview/accounts/sales/documents | Implemented |
| shop / notes / legal UI | Not implemented (notes/legal)؛ shop Partial مخفي |
| عودة آمنة من البروفايل | Implemented + Tested (allowlist) |

## مصادر الطلب

`SalesRequestSourceType` قيم مدعومة في النموذج والعرض. **مسار إدخال مربوط فعليًا؟** في هذه المرحلة: **لا** لكل القيم — لا endpoint إنشاء/استيراد ضمن النطاق. لذلك لا يُدّعى استقبال جميع المصادر؛ الطلبات تظهر إن وُجدت في الجدول بأي مصدر enum.

| SourceType | التسمية | قيمة مدعومة | مسار إدخال مربوط |
|------------|---------|-------------|------------------|
| Manual | إدخال يدوي | نعم | Not implemented |
| EmployeeSubmitted | تقديم موظف | نعم | Not implemented |
| Follower | متابع | نعم | Not implemented |
| Delegate | مندوب | نعم | Not implemented |
| ExcelImport | استيراد Excel | نعم | Not implemented |
| ExternalCrm | CRM خارجي | نعم | Not implemented |

## التبويبات والأكشنات والصلاحيات

| عنصر | صلاحية | ملف مسؤول |
|------|--------|-----------|
| قائمة غير المسند | ReadUnassigned | `SalesRequestsController` / `GetUnassignedSalesRequestsHandler` / `UnassignedSalesRequestsPage.vue` |
| بطاقات/نتائج المطابقة | MatchRead | match-summary / match-hits handlers + Matching* components |
| الماليات في النتائج/البروفايل | ReadFinancial | GetSalesRequestMatchHitsHandler / GetCustomerAuditProfileHandler |
| إسناد | Assign | AssignSalesRequestHandler |
| رفض | Reject | RejectSalesRequestHandler |
| طلب استثناء | RequestException | RequestSalesRequestExceptionHandler |
| فتح بروفايل التدقيق | ReadAuditProfile \| MatchRead \| Customer.Read | CustomersController audit-profile + CustomerAuditProfilePage |
| مستندات التدقيق | ReadAuditDocuments \| Attachment.Read | GetCustomerAuditProfileHandler documents |

وجود تطابق ≠ رفض/منع إسناد يدوي. منع الإسناد اليدوي بسبب التشابه **أُزيل**. CRM auto-assign خارج المرحلة.

## Endpoints (`/api/v1/admin`)

| Method | Path | Permission | ملاحظات |
|--------|------|------------|---------|
| GET | SalesRequests/unassigned | ReadUnassigned | فلاتر+pagination سيرفر |
| POST | SalesRequests/match-summary | MatchRead | حد 50؛ خطأ صريح عند التجاوز |
| GET | SalesRequests/{id}/match-hits | MatchRead | هوية من سجل الطلب |
| GET | SalesRequests/{id}/duplicate-requests | MatchRead | peers فقط |
| POST | SalesRequests/{id}/assign | Assign | أهلية موظف + concurrency + timeline؛ بلا Order |
| POST | SalesRequests/{id}/reject | Reject | concurrency + timeline |
| POST | SalesRequests/{id}/request-exception | RequestException | بوابات حالة + concurrency |
| GET | Customers/{id}/audit-profile | handler contracts | sections on demand |

## Endpoints App 04A (`/api/v1/app/SalesRequests`)

| Method | Path | Permission | ملاحظات |
|--------|------|------------|---------|
| GET | ?tab= | AppRead | tabs: incoming/prepared/inspected/pending/rejected/sold — هوية من الجلسة |
| GET | {requestId} | AppRead | ملكية الموظف؛ requestId ≠ orderId |
| POST | {requestId}/view | AppView | idempotent؛ Assigned→Viewed مرة واحدة |
| POST | {requestId}/prepare\|pend\|reject\|inspect | App* | أسباب حيث لزم؛ 409 تعارض |
| POST | {requestId}/link-draft | AppLinkDraft | body.orderId؛ فهرس فريد LinkedOrderId |

راجع `StateTransitions-04A.md` و `SCHEMA_REQUIRED_04A.md`. **04B–04E غير منفَّذة في هذا التسليم.**

## تغطية الاختبارات

| اختبار | الحالة |
|--------|--------|
| Matching rules unit | Tested |
| Unassigned domain rules | Tested |
| Unassigned query parity shape | Tested |
| ConcurrencyStamp increments | Tested |
| Batch limit constant/error | Tested |
| Return-context allowlist (FE) | Tested |
| Assign vs reject concurrent DB | Schema-blocked (يحتاج جدول) |
| Responsive 360/768/1440 حي | Schema-blocked؛ mock design منفصل |

## ملفات رئيسية

**Backend:** `Domain/Entities/SalesRequestAggregate/*`, `Domain/Services/SalesMatching/*`, `Application/SalesRequests/**`, `Api/.../SalesRequestsController.cs`, `Permissions.SalesRequest`, `SalesRequestConfiguration.cs`

**Frontend:** `modules/SalesRequests/**`, `modules/Customer/views/CustomerAuditProfilePage.vue`, `modules/Customer/components/AuditProfile/*`, navigation registry + useSideBar flatten
