# Acceptance 32 — اختبار النظام وتجهيز الإصدار

**تاريخ:** 2026-11-09  
**قاعدة التكامل:** `qalaat_22_23_iso` (localhost)  
**قاعدة جديدة:** `qalaat_32_fresh` (migrations فقط؛ seed عند إقلاع API)  
**HEAD committed:** `650e3a16` — الإصدار المرشح = **شجرة عمل غير مُلتزَمة** (~390 مسار)  
**نشر إنتاج:** لم يُنفَّذ (ممنوع في هذا البرومبت)

## لقطة الإصدار

انظر `evidence/32-system/release-snapshot.md`.

| فحص | نتيجة |
|---|---|
| `dotnet test` Release | **412 passed** |
| `dotnet publish` Api | OK — `Api.dll` SHA256 `ED9A9C0C…1056C167` |
| `vue-tsc` + `vite build` | OK |
| Flutter analyze | infos فقط (لا أخطاء قاتلة بعد إصلاحات المندوب) |
| Flutter APK debug | delegate + sales_employee **Build-tested** |
| Device-verified | **Missing** — لا جهاز Android مخوّل في الجلسة |

## مصفوفة نواقص 10–31 (تحقق 32)

| الوظيفة | التنفيذ الحالي | آخر دليل | تغيّر الكود بعد الدليل؟ | الاختبار المطلوب | النتيجة | يمنع نشر الوظيفة؟ |
|---|---|---|---|---|---|---|
| 10 ملاحظات مندوب | AppNotes Create + لوحة | AppNotes.create/follower + admin list=2 (32) | لا جوهريًا | API-live | **API-live PASS** | لا للمسار HTTP؛ Device Missing |
| 11 تحصيل | payments app | Coverage_11 | جزئي | Device | Device Missing | نعم للمسار الميداني الكامل |
| 12–13 موقع/مسارات متابع | لوحة + GPS | Coverage_12/13 | لا إعادة كاملة | Device/Browser | Device Missing | نعم للخلفية إن وُعدت |
| 14–20 مبيعات لوحة | وحدات جاهزة بدرجات متفاوتة | Coverage_14–20 | متفاوت | حسب الوعد | جزئي — لا إعادة شاملة بلا داعٍ | جزئي |
| 21 قانوني intake | LegalFollowups list | legal.list items=1 (32) | لا | API-live قائمة | **قائمة PASS**؛ intake مؤجل إن بقي | جزئي |
| 22 طلبات بيع | Unassigned=6 | salesRequests.unassigned (32) + Coverage_22 | لا إعادة دورة كاملة A | API-live قائمة | قائمة PASS؛ دورة كاملة Browser جزئية | جزئي |
| 23 مهام إدارية | Inbox+دورة سابقة | Coverage_23 + mgmt.tasks (32) | لا | concurrency مستقل | API سابق PASS؛ concurrency مستقل Missing | جزئي |
| 24 مهام لجنة | admin list | committee.list (32) | لا | API-live | **قائمة PASS**؛ Device Missing | جزئي |
| 25 ملاحظات تطبيقات | app+admin | AppNotes 32 | لا | API-live | **PASS** | لا للـAPI |
| 26 تنازلات | دورة مالية كاملة | `waiver-financial.json` | لا | Apply مالي | **API-live PASS** (Reject + Apply −50 على Order.SellAmount) | لا للبوابة المالية HTTP |
| 27 قرارات قانونية | LegalFollowups | legal.list 32 | لا | API-live | قائمة PASS؛ اعتماد تحويل Missing Browser | جزئي |
| 28 لوحة أسعار | list | priceboard.list 32 | لا | API-live | **قائمة PASS** (0 عناصر) | جزئي بلا إنشاء/نشر |
| 29 إعلانات | list؛ create 400 عقد جسم | announcements 32 | لا | إنشاء/نشر | list PASS؛ create يحتاج عقد جسم صحيح | جزئي |
| 30 فيديوهات | list | videos.list 32 | لا | API-live | list PASS؛ Device فتح رابط Missing | جزئي |
| 31 فرع/مفوض | زيارات/NewSales/WrongSale | evidence/31 + BranchNewSales/Payments 32 | DTO زبائن بعد 31 — أُعيد search | API/Browser | **PASS** (search/newSales/payments) | لا لهذه الطبقة |
| 31 أول تحصيل جهاز | endpoint مربوط؛ APK يُبنى | first-collection 400 بدون تكليف كامل؛ APK مبني | إصلاح بناء drawer/announcements | Device | **Build-tested**؛ Device Missing | يمنع ادّعاء Device فقط |

## دورات الأعمال (iso)

| دورة | الحالة | دليل |
|---|---|---|
| أ طلب البيع كاملة | جزئية — قائمة Unassigned مثبتة؛ دورة UI كاملة غير معاداة بالكامل هذه الجلسة | live-api-32 |
| ب قانونية | قائمة LegalFollowups | live-api-32-cycles |
| ج مهام | Inbox موجود؛ دورة 23 سابقة | Coverage_23 |
| د زيارات | أُغلقت في 31 (AlreadyOpen/Depart) | evidence/31 |
| هـ WrongSale | لوحة 31 + endpoint أول تحصيل (400 بدون بيانات تكليف كاملة) | 31 + 32 |
| و محتوى/شكاوى | AppNotes PASS؛ Announcements/Videos/Price list؛ create إعلان غير مكتمل العقد | 32 |
| تنازلات مالية | **PASS** Pending→Rejected؛ Pending→Awaiting→Apply→Approved؛ لا أثر قبل Apply؛ −50 على SellAmount؛ إعادة Apply بلا خصم ثانٍ | waiver-financial.json |

## صلاحيات (حسابات iso المتاحة)

| حساب | دور | نتيجة مختبرة |
|---|---|---|
| admin | Admin | Allow كامل على الوحدات المختبرة |
| follower_iso | Motaba | 403 على Waivers بدون صلاحية |
| mandob_iso | Mandob | AppNotes Create PASS |
| bm_iso | BranchManager | Waiver Create PASS؛ SelfDecide يمنع صاحب الطلب |
| مندوب مفوض / موظف مبيعات / بلا صلاحيات منفصل | — | **Missing كحسابات iso** (4 مستخدمين فقط) |

لا طباعة tokens/كلمات مرور في الأدلة.

## قواعد البيانات

| مسار | نتيجة |
|---|---|
| أ `qalaat_32_fresh` من migrations | Done → latest Branch31… (46) |
| ب ترقية `qalaat_22_23_iso` | نفس latest؛ بيانات حية |
| ModelSnapshot / PendingModelChangesGate | بوابة محلية فقط — لا تجاهل عام للإنتاج |
| Seed على fresh | users=0 حتى إقلاع API |
| Backup/restore معزول | لم يُنفَّذ نسخ إنتاج؛ الرجوع موثّق في Rollback_Runbook_32 |

## تطبيقات

| تطبيق | Analyze | Build | Device |
|---|---|---|---|
| delegate_app | infos | **APK debug OK** بعد إصلاح factory/import | Missing |
| sales_employee_app | infos | **APK debug OK** | Missing |
| committee_officer_application | infos | Analyze only | Missing |
| app-dev | infos كثيرة | Analyze only | Missing |
| فرع/مفوض Flutter | N/A — ويب | — | N/A |

## واجهة 360/768/1440

لم تُعاد حملة UI كاملة في 32؛ هوية 31 محفوظة. عيوب الاستخدام الجوهرية السابقة لم تُحدَّث. **Browser-live محدود** لهذه الدفعة على الوحدات المُعاد إثباتها عبر API.

## قرار قبول الدفعة

دفعة 32 **تُسلِّم إصدارًا مرشحًا + خطة نشر/رجوع** مع إثباتات حيّة للبوابة المالية والتنازلات وملاحظات التطبيقات وقوائم المحتوى و31.  
**لا إعلان «النظام كامل»** — Device وعدة Browser-live وحسابات أدوار ناقصة ما زالت Missing.
