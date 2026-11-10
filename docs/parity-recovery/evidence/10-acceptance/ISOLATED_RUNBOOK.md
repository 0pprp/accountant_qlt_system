# تشغيل معزول — قبل أي تفعيل

## قاعدة

- قاعدة: `qalaat_22_23_iso` فقط (أو نسخة fresh مُنشأة من migrations).  
- لا إنتاج في دفعة 10.  
- لا أسرار في docs/لقطات.

## خطوات

1. `git rev-parse HEAD` وسجّل الـhash في GO_NO_GO.  
2. API:  
   `dotnet ef database update --project src/Infrastructure --startup-project src/Api` على iso.  
3. اختبارات:  
   `dotnet test` (exit code حقيقي).  
   `npm test` في panel.  
   `flutter test` لكل تطبيق معني.  
4. API-live سكربتات `evidence/03`…`08` + POST SalesExcelSearch بعد login sm_iso.  
5. Backup: `pg_dump` قبل التفعيل؛ استعادة = مسار الرجوع الآمن للبيانات.  
6. Artifacts: publish بدون appsettings أسرار؛ سجّل SHA256.

## رجوع

- كود: نشر الـcommit السابق.  
- بيانات مالية: استعادة dump — ليس EF downgrade.
