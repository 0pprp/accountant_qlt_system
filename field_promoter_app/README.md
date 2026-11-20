# المروّج الميداني — قلعة الضمان

تطبيق Android (Flutter) للمروّجين الميدانيين: إرسال طلبات البيع، تثبيت المحلات على الخريطة، متابعة الطلبات وتسديد المبيعات، وتسجيل الدوام مع تتبّع الموقع.

عقد الـ API: `docs/field-promoter/api-contract.md` (المسار `api/v1/app/field-promoter`).

## التشغيل

```bash
cd field_promoter_app
flutter pub get
flutter run                      # debug: الخادم الافتراضي http://10.0.2.2:15999/
flutter test
flutter analyze
flutter build apk --release      # release: https://api.qalataldhaman.com/
```

## الإعدادات

| المتغير | الافتراضي | الوصف |
|---|---|---|
| `--dart-define=API_BASE_URL=https://host/` | debug: `http://10.0.2.2:15999/`، release: `https://api.qalataldhaman.com/` | عنوان الخادم |
| `--dart-define=REQUIRE_HTTPS=true` | `false` في debug | فرض HTTPS في debug (في release مفروض دائماً) |

- مفتاح التحديث داخل التطبيق: `field-promoter` (`GET api/v1/app/mobile-updates/field-promoter`) عبر الحزمة المشتركة `packages/qalaat_in_app_update`.
- الرموز (access/refresh) محفوظة في `flutter_secure_storage` فقط. مسودة الطلب وطابور نقاط الموقع في `shared_preferences` (بلا رموز)، وتُمسح عند تسجيل الخروج.
- الخرائط: OpenStreetMap عبر `flutter_map` بـ `userAgentPackageName = iq.qalaa.field_promoter`. المحلات تُحمَّل للمربع الظاهر فقط عند تكبير 12 فأكثر، مع تأخير بعد تحريك الخريطة.

## التوقيع (release)

ضع `android/key.properties` (غير مرفوع إلى git) بهذا الشكل:

```properties
storeFile=C:/path/to/field_promoter.jks
storePassword=...
keyAlias=...
keyPassword=...
```

إن لم يوجد الملف يُبنى الـ APK بتوقيع debug (للتجربة فقط، لا يصلح للنشر أو للتحديث فوق نسخة موقّعة).

## الصلاحيات (Android)

| الصلاحية | السبب |
|---|---|
| `INTERNET` | الاتصال بالخادم |
| `ACCESS_FINE_LOCATION`, `ACCESS_COARSE_LOCATION` | تثبيت المحلات وتسجيل مسار الدوام |
| `FOREGROUND_SERVICE`, `FOREGROUND_SERVICE_LOCATION`, `WAKE_LOCK` | تتبّع الموقع أثناء الدوام مع إشعار ثابت «الدوام مستمر — يتم تسجيل موقعك أثناء الدوام» |
| `POST_NOTIFICATIONS` | إظهار إشعار التتبّع (Android 13+) |
| `CAMERA` | تصوير المحل (اختياري) |
| `REQUEST_INSTALL_PACKAGES` | تثبيت التحديث الإلزامي داخل التطبيق |

لا يطلب التطبيق `ACCESS_BACKGROUND_LOCATION` (مثل تطبيق مسؤول اللجنة)؛ التتبّع يعمل عبر خدمة أمامية بدأت والتطبيق مفتوح.

## سلوكيات مهمة

- **الدوام**: يبدأ بزر «بدء الدوام» أو تلقائياً مع أول تثبيت محل أو طلب. النقاط تُجمع كل 20 ثانية تقريباً (مع الدقة و`clientEventId` لكل نقطة) وتُرسل دفعةً كل 60 ثانية أو عند 200 نقطة. تُحذف النقاط من الطابور فقط بعد أن يؤكد الخادم كل نقاط الدفعة (مقبولة/مكررة/مرفوضة)، ويتوقف التتبّع إذا ردّ الخادم `isWorking=false`.
- **الطلب**: مفتاح `submissionKey` واحد لكل محاولة؛ يبقى عند إعادة المحاولة بعد انقطاع أو خطأ 5xx، ويتجدد بعد النجاح أو عند تغيير محتوى النموذج. الصورة JPEG (عرض 1600، جودة 72) بنوع `image/jpeg`.
- **الجلسة**: فشل تجديد الرمز بسبب الشبكة يُظهر «لا يوجد اتصال بالإنترنت» ولا يُخرج المستخدم؛ فقط رفض الخادم (400/401/403) يعني انتهاء الجلسة.
- رسائل الخادم العربية تُعرض كما هي.
