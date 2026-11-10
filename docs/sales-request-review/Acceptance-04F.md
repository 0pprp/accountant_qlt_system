# Acceptance-04F — إغلاق دورة المبيعات والتحقق التشغيلي

**لا يُعلن اكتمال كل شيء.** هذا الملف يفصل ما نُفّذ وما ثبت حيًا على القاعدة المعزولة `qalaat_04f_sales_verify` (@ `localhost`/`::1:5432`) عن ما بقي Live-blocked أو Not implemented.

## بيئة الإثبات

| عنصر | قيمة |
|------|------|
| Host/DB قبل أي SQL | `::1:5432` / `qalaat_04f_sales_verify` (تحقق `current_database()` + `inet_server_*`) |
| Schema | clone من مخطط تطبيقي + `SCHEMA_04F_apply_bundle.sql` + `docs/unified-admin-permissions/_apply_phase02_schema.sql` (`UserPermissionDenials`) — ملفات 04A/04D/04E وحدها لا تكفي لقاعدة فارغة |
| Migration | لم تُنشأ؛ `PendingModelChanges` تجاهل مقيّد بـ `Phase02LocalVerify:AllowedDatabase=qalaat_04f_sales_verify` فقط |
| API | `http://127.0.0.1:4200` ضد القاعدة المعزولة |
| Panel | Vite `http://127.0.0.1:5174` + `.env.04f.local` أثناء الجلسة |
| إنشاء SalesRequest | SQL موثّق (`04f-live-evidence/_seed_*.sql`) — **ليس** إثبات ingestion |
| أدلة | `docs/sales-request-review/04f-live-evidence/` |

## حالة البناء (04F)

| سطح | نتيجة | ملاحظات |
|-----|--------|---------|
| Backend `dotnet test` SalesRequests | Unit-tested / Integration-tested (InMemory) | لا يغني عن PostgreSQL |
| Frontend `vue-tsc` + vitest | Unit-tested | |
| Flutter analyze | infos/warnings سابقة (pre-existing) | لا أخطاء جديدة مفروضة لهذه المرحلة |
| Flutter tests | 24 passed | |
| PostgreSQL relational | Live-verified | Unique `LinkedOrderId` OK؛ FK RESTRICT SQLSTATE `23001`؛ outbox `EventId` unique |
| Permissions seed | Live-verified | Admin: 28 OutsideAll؛ Mandob: 7 App*؛ ChiefAccountant: **0** SalesRequest (ليست عبر Permissions.All) |

## مفردات الحالة

| Status | المعنى |
|--------|--------|
| Implemented | يوجد مسار كود يبني |
| Unit-tested | اختبار وحدة يمر |
| Integration-tested | تدفق handlers على InMemory و/أو تحقق علائقي PG |
| Live-verified | مارس على API/DB/متصفح أو عميل SignalR حقيقي في هذه الجلسة |
| Live-blocked | يحتاج بيئة/جهاز لم يُثبت بالكامل |
| Not implemented | لا مسار |

---

## 1) التحقق العلائقي (بديل InMemory)

| قيد | Status | دليل |
|-----|--------|------|
| Unique partial `LinkedOrderId` | Live-verified | NOTICE `UNIQUE_LINKEDORDER_OK` |
| FK `LinkedOrderId` RESTRICT | Live-verified | SQLSTATE `23001` يمنع حذف Order المربوط |
| Outbox `EventId` unique + publish/recover | Live-verified | صف 14: RetryCount=20 ثم reset → `pub` خلال ~4s |
| أعمدة 04E / timeline / outbox موجودة | Live-verified | `_tmp_04f_schema_verify.sql` |

## 2) واجهة الإجراءات (Panel)

| إجراء | API | Panel UI | Status |
|-------|-----|----------|--------|
| تفاصيل الاستثناء + حالة الاستهلاك | Implemented | RequestDetailsPanel + lifecycle | Implemented + Live-verified (browser: حالة مرفوض + سبب + سجل) |
| موافقة/رفض استثناء + سبب | Implemented | RequestLifecycleActions (فقط إن `pending` + صلاحية) | Implemented + Unit-tested؛ Live-verified عبر API (decider≠requester)؛ UI تظهر الحالة لا أزرار بعد القرار |
| Soft-delete + سبب + أثر | Implemented | lifecycle + confirmation | Implemented؛ Live-verified API؛ UI تعرض كتلة الحذف دون زر وهمي للإجراءات غير المنفذة |
| Restore | Implemented | على تبويب المحذوفة | Implemented + Live-verified API |
| القانونية | Not implemented | زر معطّل «غير منفَّذ» | Not implemented (متعمد) |
| المرسلة | Implemented (فلتر فقط) | caveat / emptyHint — **لا ادعاء ingestion** | Implemented؛ Live-verified counts caveat=`no_ingestion_path_wired`؛ قائمة فارغة ≠ «بحث بلا نتائج مربوط» |

## 3) مسار A/B الحي (API)

| بند | Status | دليل مختصر |
|-----|--------|-------------|
| إنشاء طلب بآلية اختبار موثقة | Live-verified | SQL seed — ليس ingestion |
| إسناد إلى A | Live-verified | بعد `CreationStep=Completed` للMandob |
| يظهر لـ A لا لـ B | Live-verified | `A_incoming` فيه #3؛ `B_incoming` = `[]` |
| view → prepare → inspect | Live-verified | timeline + counts |
| ensure-draft×2 بدون Order إضافي | Live-verified | `LinkedOrderId=4` مرتين؛ `Orders` count=1 |
| إعادة إسناد إلى B ومنع A | Live-verified | `SalesRequest_Not_Owner` / NotFound لـ A |
| إكمال البيع يربط الطلب الصحيح | Live-verified | complete Order#4 → SalesRequest#3 Status=`9` Completed |
| استثناء approve/reject + self-approve مرفوض | Live-verified | Self → `Exception_Self_Approval`؛ decider نجح |
| soft-delete / restore | Live-verified | |
| سحب صلاحية AppRead | Live-verified جزئيًا | JWT القديم 200؛ بعد login جديد **403** (claims) |
| انعكاس اللوحة دون reload (SignalR UI) | Live-blocked / Partial | اللوحة أظهرت «غير متصل (polling)»؛ التحديث عبر polling محدود لا يغني عن إثبات UI↔hub |

## 4) Realtime

| بند | Status | قياس/ملاحظة |
|-----|--------|-------------|
| اتصال فعلي Origin→Hub | Live-verified (عميل Node بنفس `@microsoft/signalr`) | Origin `http://127.0.0.1:5173`؛ transport **WebSockets**؛ negotiate CORS `*` |
| CORS | Live-verified | Development `AllowAnyOrigin`؛ negotiate من Origin اللوحة 200 |
| JWT وحده لا يكفي | موثّق | حزمة + access_token لا تُعلَن نجاحًا؛ قيس الاتصال |
| قطع/إعادة اتصال + RefreshSubscriptions | Live-verified | groups=2 بعد reconnect |
| زمن التحديث save→event | **مقيس** | **336 ms** ثم **1466 ms** (لا ادّعاء ≤2s بدون دليل؛ كلاهما ≤2s في هذه العيّنة) |
| duplicate/out-of-order | Implemented (عميل) | dedup eventId + رفض revision أقدم — لم يُعاد هجوم اصطناعي كامل على UI |
| سحب صلاحية/فرع أثناء اتصال Hub | Live-blocked | API deny بعد re-login مثبت؛ قطع مجموعات Hub تحت اتصال قائم لم يُقَس من اللوحة |
| فشل نشر بعد commit ثم استعادة outbox | Live-verified | صف outbox RetryCount=20 ثم recovery→pub |
| لوحة Vue متصلة في المتصفح المضمّن | Live-blocked | UI: disconnected + fallback poll؛ لا موارد negotiate في شريط الأداء للمتصفح المضمّن |

## 5) التصميم / الشاشات

| بند | Status |
|-----|--------|
| Panel 1440 / 768 / 360 من البيئة المعزولة | Live-verified — `04f-live-evidence/screenshots/` |
| وضوح الحالة/الموظف/آخر تغيير/السبب | Live-verified على تفاصيل الاستثناء |
| القانونية غير جاهزة بصريًا | Live-verified (disabled) |
| Flutter على محاكي/جهاز لدورة الطلب الكاملة | Live-blocked — أجهزة Windows/Chrome/Edge متاحة؛ لم تُشغَّل جلسة تطبيق كاملة ضد API المعزول في هذه الجولة (analyze+24 tests فقط) |

## 6) ملخص صريح

- **Implemented + live API path** لدورة الإسناد/الانتقال/ensure-draft/الإكمال/الاستثناء/الحذف على PostgreSQL المعزول.
- **Realtime مقيس** عبر عميل SignalR حقيقي؛ **لوحة المتصفح لم تُثبت متصلة** في هذه الجلسة.
- **Legal / ingestion للمرسلة: Not implemented.**
- **لا عبارة «اكتمل كل شيء».**
