# تطبيق مسؤول اللجنة

Flutter app (`com.qalaat.committee_officer`) بهوية فريق قلعة الضمان، وبزنس مطابق للمتابع (`follower_application`).

## قواعد الواجهة

- كل صفحة داخل `Scaffold` + `AnimatedWavesBackground`
- الحوارات عبر `showCenteredDialog` فقط (وسط الشاشة)
- الشِل: `PageView` + `GlassyBottomBar` مع شريط تعويض `MediaQuery.padding.bottom` عند `extendBody: true`
- لا محتوى تحت أزرار نظام أندرويد

## التشغيل

```bash
flutter pub get
flutter run --dart-define=API_BASE=http://127.0.0.1:5080/
```

## API

AppArea JWT: `api/v1/app/committee/*` داخل `api-main`.
توثيق: `docs/CommitteeOfficer_API_Endpoints.xlsx` و `docs/CommitteeOfficer_Project_Paths_and_Architecture.docx`.
