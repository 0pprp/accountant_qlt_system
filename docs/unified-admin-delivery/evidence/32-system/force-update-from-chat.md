# رفع تحديث إجباري من المحادثة (ليس من اللوحة)

التطبيقات تفحص: `GET /api/v1/app/mobile-updates/{appKey}?currentVersionCode=`

مفاتيح: `delegate` | `sales-employee` | `committee` | `follower`

عند طلب المستخدم «ارفع تحديث إجباري»:
1. ابنِ APK بالنسخة الجديدة (versionName + versionCode في pubspec).
2. انشر سجل الإصدار عبر API إداري أو SQL على `MobileAppReleases`:
   - ارفع `LatestVersionCode` فوق المثبت
   - `ForceUpdate = true`
   - `MinimumSupportedVersionCode` حسب السياسة
   - `ApkUrl` صالح http(s)
3. الأجهزة القديمة تُجبر على التحديث قبل المتابعة.

لا توجد شاشة إدارة في اللوحة لهذا الغرض.
