# مخطط قاعدة البيانات المطلوب — مرحلة 02

**لم يُولَّد ملف migration** التزامًا بـ `api-main/AGENTS.md`.

## 1) عمود الإصدار على المستخدم

```sql
ALTER TABLE "Users"
  ADD COLUMN IF NOT EXISTS "PermissionsVersion" integer NOT NULL DEFAULT 0;
```

يُعامل في EF كـ `IsConcurrencyToken()`.

## 2) جدول المنع الخاص

```sql
CREATE TABLE IF NOT EXISTS "UserPermissionDenials" (
  "UserId" integer NOT NULL,
  "PermissionId" integer NOT NULL,
  CONSTRAINT "PK_UserPermissionDenials" PRIMARY KEY ("UserId", "PermissionId"),
  CONSTRAINT "FK_UserPermissionDenials_Users_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_UserPermissionDenials_Permissions_PermissionId"
    FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE RESTRICT
);
```

## 3) بذرة الصلاحية والدور

- إدراج `User.ManagePermissions` في جدول `Permissions` إن لم يوجد.
- ربطها بدور `Admin` فقط في `RolePermissions` (ليس لـ ChiefAccountant / MainAccountant رغم امتلاكهما بقية `Permissions.All`).

## قيد التشغيل

لا تعتبر الميزة جاهزة على قاعدة موجودة قبل تطبيق المخطط أعلاه واختبار PATCH المتزامن وlogin/refresh.

## فجوة ModelSnapshot ومسار الإنتاج

- تطبيق SQL اليدوي (`_apply_phase02_schema.sql` / الأوامر أعلاه) **لا يحدّث** `ApplicationDbContextModelSnapshot` ولا يضيف migration.
- لذلك يبقى نموذج EF «أمام» تاريخ الـmigrations إلى أن تُسوّى الفجوة.
- **مسار ترقية الإنتاج** (قبل النشر، وفق `api-main/AGENTS.md`):
  1. تسوية الفجوة بسياسة المشروع المعتمدة (migration رسمي عند السماح، أو إجراء موافق عليه صراحةً) بحيث يطابق الـsnapshot المخطط الفعلي.
  2. تطبيق المخطط على قاعدة الهدف بعد المراجعة.
  3. التحقق من PATCH المتزامن وlogin/refresh.
- **لا يُولَّد migration ضمن مرحلة التحقق المحلية** ما دامت السياسة تمنع ذلك.

## تجاهل PendingModelChangesWarning

- **الافتراضي: غير مفعّل** في الإعدادات العادية (Development أو Production).
- التفعيل الاختياري للتجربة المحلية فقط عند اجتماع الشروط كلها:
  - `ASPNETCORE_ENVIRONMENT=Development`
  - `Phase02LocalVerify:AllowPendingModelChanges=true` (افتراضي `false`)
  - اتصال PostgreSQL بحلقة محلية (`localhost` / `127.0.0.1` / `::1`) **و** قاعدة مسموحة (`Phase02LocalVerify:AllowedDatabase`، الافتراضي `qalaat_phase02_verify`)
- اسم القاعدة وحده لا يكفي؛ المضيف غير المحلي يرفض التجاهل.

## تحقق تشغيلي محلي (2026-11-04)

- طُبّق على قاعدة معزولة فقط: `qalaat_phase02_verify` @ `127.0.0.1` (ليست إنتاجًا).
- الطريقة: `dotnet ef database update` للمigrations الحالية + `_apply_phase02_schema.sql` (بدون توليد migration).
- نتائج الربط الحي: `_phase02_live_results.json` (24/24) + لقطات `screenshots/`.
- أُغلق التجاهل العام؛ أي إعادة تشغيل محلي للاختبار يحتاج الشروط أعلاه صراحةً.

## تصحيح PowersForm (بدون DB)

- `expectedVersionForSave` = إصدار `baseline` المعتمد فقط؛ `serverLatest` / «مقارنة فقط» لا يغيّران رقم الحفظ.
- اعتماد الإصدار الجديد فعل صريح: دمج محلي فوق الخادم + حسم تعارضات؛ ثم تحديث `baseline` و`draft` معًا.
- واجهة الموظف: رسائل عربية بدل أرقام baseline/server؛ التفاصيل في `console.debug` / expose للتشخيص.
- شريط الإجراءات الثابت: `padding-bottom` كافٍ على الهاتف حتى لا تُحجب آخر الخيارات.
