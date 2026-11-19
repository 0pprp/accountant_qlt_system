# تبسيط إنشاء الحسابات — تسليم 2026-11-16

## الملفات المعدّلة / المضافة

### لوحة
- `panel-admin-main/.../User/views/UserCreatePage.vue` — تركّب `UserCreateForm`
- `panel-admin-main/.../User/components/UserCreateForm/UserCreateForm.vue` — نموذج واحد RTL
- `panel-admin-main/.../User/types/api/index.ts` — `UserCreatePayload`
- `panel-admin-main/.../User/localization/ar.json` — تصحيح «اختر الفرع»
- `panel-admin-main/.../Core/components/ui/AppInput.vue` — aria إظهار/إخفاء كلمة السر
- حذف `UserCreateStepper/` (بلا مستهلك بعد الفصل)

### API
- `Domain/.../User.cs` — حقول شخصية اختيارية؛ `CreationStep=Completed` عند الإنشاء
- `UserConfiguration.cs` — nullability + فهرس فريد `Username`
- `CreateUserCommand` / `Validator` / `Handler`
- `UpdateUserCommand` / `Validator` (اختيارية متوافقة)
- `GetUserByIdResponse` / `GetUserProfileResponse` — أنواع اختيارية
- Migration: `20261116171245_OptionalUserPersonalFields`

### وثائق
- `docs/management-rollout/Architecture_File_Map_01.md`
- `docs/management-rollout/Management_Endpoints.xlsx` (صف POST Users)
- `docs/management-rollout/reference.json`

## اختبارات
- Backend: `CreateUserCommandTests` — 6 نجحت
- Vitest: 105 نجحت
- `vue-tsc` + `vite build` — نجحا

## تحقق حي
- لم يُطبَّق مخطط الإنتاج.
- Migration جاهزة للتطبيق على قاعدة معزولة فقط.
- لقطات 360/768/1440 وPOST حي: معلّقة حتى جلسة لوحة+API على المعزولة.
