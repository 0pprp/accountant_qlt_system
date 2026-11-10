# تحديث إجباري — داخل التطبيق (1.0.2+3)

## تجربة التطبيق

التنزيل والتثبيت داخل التطبيق فقط عبر `qalaat_in_app_update`:
فحص → شاشة كاملة → تنزيل مع تقدّم → مثبت النظام.
لا يوجد زر تنزيل عبر المتصفح في المسار الأساسي.

## كود

- فرع `development` يتضمن الحزمة `packages/qalaat_in_app_update`
- التطبيقات: `delegate` / `sales-employee` / `committee` → **1.0.2+3**

## APK Release

https://github.com/0pprp/accountant_qlt_system/releases/tag/mobile-1.0.2-3

| AppKey | Version | Code | SHA256 | APK |
|---|---|---|---|---|
| delegate | 1.0.2 | 3 | `969DE7E0D9B81639E4BD4D6ADE982599C73B4D2EC5BA1FAF8EB2249C7C9ED13D` | [delegate](https://github.com/0pprp/accountant_qlt_system/releases/download/mobile-1.0.2-3/delegate-1.0.2%2B3.apk) |
| sales-employee | 1.0.2 | 3 | `8E1D492F222729339D156C315AF4C6570FE89719F8137C7F25BC12A61761C81F` | [sales-employee](https://github.com/0pprp/accountant_qlt_system/releases/download/mobile-1.0.2-3/sales-employee-1.0.2%2B3.apk) |
| committee | 1.0.2 | 3 | `96E4F8A5038636A49D94E96DEA17D19A652293FF1E577AEBD7DC5C74F1E817D6` | [committee](https://github.com/0pprp/accountant_qlt_system/releases/download/mobile-1.0.2-3/committee-1.0.2%2B3.apk) |

## تفعيل الإنتاج (يحتاج Admin إنتاجي)

`PUT /api/v1/admin/MobileAppReleases/{appKey}` مع:

- `latestVersionCode = 3`
- `minimumSupportedVersionCode = 3`
- `forceUpdate = true`
- `apkUrl` من الجدول أعلاه
- `sha256` من الجدول أعلاه

تحقق: `GET /api/v1/app/mobile-updates/{appKey}?currentVersionCode=1`
→ `forceUpdate=true`, `latestVersionCode=3`, `apkUrl` غير فارغ.

> ملاحظة: أول انتقال من نسخة قديمة ما زالت تستخدم المتصفح قد يفتح المثبت عبر المسار القديم مرة واحدة؛ بعد تثبيت 1.0.2+3 تصبح التجربة داخل التطبيق فقط.
