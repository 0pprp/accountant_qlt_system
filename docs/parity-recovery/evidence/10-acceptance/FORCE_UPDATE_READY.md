# تحديث إجباري — جاهز للنشر على الإنتاج

## تم بالفعل

1. كود `development` مرفوع:
   - https://github.com/0pprp/accountant_qlt_system/commit/a9942f753fe1c15bb611685f5da56bc8c9a05123
   - https://github.com/0pprp/accountant_qlt_system/commit/5f7e48e (bump 1.0.1+2)
2. Release APKs عامة:
   - https://github.com/0pprp/accountant_qlt_system/releases/tag/mobile-1.0.1-2

| AppKey | Version | Code | SHA256 | APK |
|---|---|---|---|---|
| delegate | 1.0.1 | 2 | `9A3F6BAEFD45473B0484313B53CF11CC195CCF580B34FA7A769975A2C8152568` | [delegate](https://github.com/0pprp/accountant_qlt_system/releases/download/mobile-1.0.1-2/delegate-1.0.1%2B2.apk) |
| sales-employee | 1.0.1 | 2 | `9B1EF8CDE293830A24E2D0269B9335CBC3CCB2AA0EFC17FCBA306B09CC59F35C` | [sales-employee](https://github.com/0pprp/accountant_qlt_system/releases/download/mobile-1.0.1-2/sales-employee-1.0.1%2B2.apk) |
| committee | 1.0.1 | 2 | `13D29BDCFBB2A0259D6AAD2BB781413054A6C631BF0F23184492092FF53DFAFD` | [committee](https://github.com/0pprp/accountant_qlt_system/releases/download/mobile-1.0.1-2/committee-1.0.1%2B2.apk) |

## متوقف عند نشر سياسة الإنتاج

محاولة `POST /api/v1/admin/Auth/login` على `api.qalataldhaman.com` ببيانات `AdminData__*` المحلية → **400 اسم/كلمة مرور غير صحيحة**.  
لا يمكن تفعيل `ForceUpdate=true` على الإنتاج بدون حساب Admin إنتاجي يملك `MobileAppRelease.Publish`.

## أمر التفعيل (بعد توفير توكن إنتاج)

```powershell
$token = "<ACCESS_TOKEN>"
$headers = @{ Authorization = "Bearer $token" }
# لكل تطبيق: PUT /api/v1/admin/MobileAppReleases/{appKey}
$body = @{
  latestVersionName = "1.0.1"
  latestVersionCode = 2
  minimumSupportedVersionCode = 2
  forceUpdate = $true
  apkUrl = "https://github.com/0pprp/accountant_qlt_system/releases/download/mobile-1.0.1-2/delegate-1.0.1%2B2.apk"
  sha256 = "9A3F6BAEFD45473B0484313B53CF11CC195CCF580B34FA7A769975A2C8152568"
  releaseNotes = "تحديث إجباري 1.0.1+2"
} | ConvertTo-Json
Invoke-RestMethod -Method Put -Uri "https://api.qalataldhaman.com/api/v1/admin/MobileAppReleases/delegate" -Headers $headers -ContentType "application/json; charset=utf-8" -Body ([Text.Encoding]::UTF8.GetBytes($body))
```

تحقق: `GET https://api.qalataldhaman.com/api/v1/app/mobile-updates/delegate?currentVersionCode=1`  
المتوقع: `forceUpdate=true`, `latestVersionCode=2`, `apkUrl` غير فارغ.
