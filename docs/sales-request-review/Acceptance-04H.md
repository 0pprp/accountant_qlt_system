# Acceptance-04H — تشغيل تطبيق الموظف وجرد الكارت

يبني على 04F/04G. **لا إعادة تأسيس Backend أو Vue Hub.** Legal وingestion يبقيان Not implemented.

## هدف Flutter الذي اشتغل

| هدف | الحالة | دليل |
|-----|--------|------|
| Android هاتف/محاكي | **Live-blocked** | لا جهاز؛ cmdline-tools/licenses ناقصة؛ لا emulator جاهز — **ليس Mobile-verified** |
| Windows desktop | **Live-blocked** | VS Desktop C++ toolchain ناقص — **لم يُثبَّت Visual Studio عمدًا** |
| Web (`web-server` :5180) | **Web shell verified** | login emp_a؛ بدء دوام (GPS stub Web — **الموقع غير مثبت**); الرئيسية + عدّادات Dio؛ تنقل «طلبات البيع» → `SalesRequestsBinPage`؛ الدرج يحتفظ بـ «سجلات البيع» = Orders |

## نتيجتان منفصلتان (لا تُخلطا)

| النتيجة | الحالة | ماذا تغطي |
|---------|--------|-----------|
| **Web shell verified** | **محقّق** | Login، هيكل الرئيسية، صناديق العدّادات عبر Dio، مداخل طلبات البيع → SalesRequests (ليس Orders)، SessionStore Web (memory + SharedPreferences) بدل DomException على secure storage |
| **Sales lifecycle verified** | **محقّق على Web عبر عميل Flutter** | طلب #13: اطلاع → تجهيز → كشف → ensure-draft/معالج إتمام → **تم البيع** من UI؛ لوحة Vue تبويب تم البيع تعرض #13 مع Hub «متصل» دون الاعتماد على reload كشرط؛ طلب #14 إعادة إسناد إلى Employee B من اللوحة وغياب #14 من صندوق emp_a؛ فشل الشبكة يعرض رسالة + «إعادة المحاولة» بلا بيانات وهمية |

> **GPS:** بدء الدوام استعمل `LocationService.webFallback` — لا يُعتبر إثبات موقع حي.

## تشخيص Dio (جذر العطل السابق)

| مرحلة | السبب | الإصلاح |
|-------|--------|---------|
| قبل الإرسال | `FlutterSecureStorage` على Web يرمي DomException عند قراءة JWT → `session_read_failed` | `SessionStore`: Web = memory + SharedPreferences؛ native try/catch |
| إرسال/ربط | `Content-Type` على GET يفسد preflight | sanitize في Dio interceptor + اختبار |
| إكمال البيع | `attachmentType` بدل `Type`؛ `fromFile(blob)` على Web؛ إرسال `orderListId` لمندوب | Form field `Type`؛ `MultipartFile.fromBytes`؛ `orderListId: null` لـ Mandob |

اختبارات انحدار: `session_store_memory_test`، `dio_failure_probe_test`، `sales_nav_test`، `attachment_upload_form_test`.

## دليل السيناريو (UI Flutter + لوحة)

| خطوة | الحالة | ملاحظة |
|------|--------|--------|
| تنقل طلبات البيع | Verified | Bottom nav + صناديق الرئيسية → `/sales-requests/...`؛ Orders تبقى «سجلات البيع» |
| عدّادات Dio | Verified | بعد إصلاح الجلسة تظهر أعداد حقيقية (لا demo) |
| تجهيز / كشف / إتمام | Verified (Flutter UI #13) | لقطات `c13_*`؛ status API tab=sold بعد تم البيع من المعالج |
| انعكاس اللوحة | Verified | تبويب تم البيع يظهر #13؛ status Hub متصل |
| إعادة إسناد ومنع السابق | Verified | #14 → Employee B من حوار اللوحة؛ صندوق emp_a لا يعرض #14 |
| فشل شبكة | Verified | offline → حوار خطأ + شاشة مع «إعادة المحاولة» |

## CardParityMatrix

- `docs/sales-request-review/CardParityMatrix.md`
- `docs/sales-request-review/CardParityMatrix.csv`
- محفوظة لمراجعة التصميم التالية (مع لقطات 1440/360 السابقة)

## لقطات

`docs/sales-request-review/04h-live-evidence/screenshots/` — panel sold #13، reassign #14، Flutter complete success، network retry، إضافةً لقطات الـ shell السابقة.

## Go / No-Go

- **Go** لـ **Web shell verified** و**Sales lifecycle verified** على Web ضد القاعدة المعزولة `qalaat_04f_sales_verify`.
- **No-Go** لاعتبار **Mobile-verified** أو إثبات GPS حي، أو Legal/ingestion.
