# Deployment Runbook 32

**لا ينفَّذ نشر إنتاج بهذا المستند في دفعة 32 — خطة فقط.**

## 1) Artifacts وبصمات (مرشح 2026-11-09)

| Artifact | الموقع | SHA256 |
|---|---|---|
| Api.dll | `evidence/32-system/artifacts/api-publish/Api.dll` | `ED9A9C0CD542841ED77037BED14738DDB7E3E2354DBBD8403E0484291056C167` |
| Application.dll | نفس المجلد | `DC94CE412FA62B19CED7A63903AE14D0FDB122EC1A392963BAFF577021ABA767` |
| Panel index chunk | `panel-admin-main/.../dist/js/index-CQLR3Srj.js` | `CB836CDFC8CF91D0194832BA85D3FBA3D06C123D2C3A1C3F423311DBFEE9C9DF` |
| sales_employee debug APK | `sales_employee_app/.../app-debug.apk` | `28C46E0CA3EE3CB155B6BE77959C33CC309D3D90FC3DF05D315FF30FD979593B` |
| delegate_app debug APK | `delegate_app/.../app-debug.apk` | `575570D124C236DCC1C5386EB03EB0B34D110067C9111B1AAA8F2EB92D85589C` |

أعد حساب البصمات بعد أي rebuild. لا تُضمِّن `.env` أو مفاتيح توقيع.

## 2) إعدادات بيئة لازمة (بدون أسرار)

- `ConnectionStrings__Postgres`
- `ASPNETCORE_ENVIRONMENT` (Staging أولاً)
- JWT / Auth options عبر مخزن أسرار المنصة
- CORS أصول اللوحة/التطبيقات الفعلية فقط — لا توسعة عشوائية
- URL قاعدة API للتطبيقات عبر إعداد بيئة البناء (`--dart-define` / flavors) — التحقق من غياب localhost في build الإصدار
- مسار التخزين/Attachments ثابت خارج حزمة الاستبدال

## 3) ترتيب التحديث

1. **Backup** قاعدة الهدف (منفصل عن الإنتاج إن كان staging).
2. **DB migrate** إلى latest: `20261109184704_Branch31VisitMembersAttachmentsAndWrongSaleSource` (46 إجماليًا في المرشح).
3. فحص `__EFMigrationsHistory` + smoke SQL للقيود.
4. **API** نشر ملفات publish دون مسح uploads.
5. إقلاع API → انتظار seed/permissions إن كانت قاعدة جديدة.
6. **Panel** نشر `dist/` خلف نفس بوابة الـAPI المتوقعة.
7. **Apps** توزيع APK/IPA الموقّعة (ليست debug) بعد Device smoke.
8. إبطال جلسات عند تغيّر سياسة الأدوار إن لزم (إعادة دخول).

## 4) فحص الترقية

- مسار أ: قاعدة فارغة + migrate + إقلاع + login admin.
- مسار ب: نسخة اختبار من مخطط الإصدار السابق → migrate → مقارنة عدد migrations والوحدات الحرجة.
- لا سحب بيانات إنتاج محليًا دون تفويض.

## 5) Readiness / Smoke

- Login admin + حساب محدود.
- `GET /admin/Waivers`، `BranchVisits/active`، `BranchNewSales`، `AppNotes`، `SalesRequests?status=Unassigned`.
- إنشاء ملاحظة تطبيق من مندوب اختبار.
- Realtime: اتصال لوحة؛ عند الفشل استخدم polling المعلن فقط.
- Panel: آباء التنقل الأربعة بدون حلقات؛ قفل مقابل عدم التوفر.

## 6) توافق التطبيقات القديمة

- عملاء قديمون قد يفشلون على حقول WrongSale/Visit الجديدة — خطّة ترحيل قصيرة أو إجبار تحديث.
- ما يحتاج إعادة دخول: تغيّر الدور/الفروع/سحب صلاحية أثناء الجلسة.

## 7) ميزات تُبقى غير مفعّلة إن بقي مانع

- وعد Device GPS خلفية.
- نشر إعلانات/فيديوهات واسع قبل تثبيت عقد الإنشاء وBrowser.
- أي مسار مالي إضافي خارج بوابة Waiver المثبتة دون مراجعة محاسب.

## 8) ما بعد النشر (staging)

- حفظ بصمات البيئة المنشورة.
- تحديث سجل الإصدار وربطه بـ commit/tag نظيف (مُستحسن قبل إنتاج).
