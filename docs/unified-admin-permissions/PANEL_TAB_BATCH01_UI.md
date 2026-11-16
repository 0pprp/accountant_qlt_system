# دفعة 01 — واجهة صلاحيات التبويبات (لوحة الويب)

تاريخ: 2026-11-16  
النطاق: ربط اللوحة بعقود `tab-access` + محرر RTL + السايدبار/الحراسة  
نشر إنتاج: **لم يُنفَّذ**

## مسار القرار: المفتاح → API

1. المدير يبدّل مفتاح تبويب في `PowersForm.vue` (مسودة محلية).
2. الحفظ يستدعي `PATCH /api/v1/admin/Users/{id}/tab-access` عبر `usePatchTabAccessMutation`.
3. الخادم يحدّث allow/deny/inherit لـ `Panel.Tab.{tabKey}` ويرفع `PermissionsVersion` ويبطل الجلسات (`SecurityStamp`).
4. عند دخول الحساب: `AuthService.GroupPermissionsForClient` يضع `Panel.Tab.*` تحت وحدة `PanelTab`، و`EffectivePermissionCalculator` يوسّع التبويب المفتوح إلى `GrantedActions`.
5. السايدبار و`resolveRouteAccess` و`useManagementAccess` يقيّمون `PanelTab.{tabKey}` عبر `panelTabAccess.ts`.
6. سحب التبويب أثناء الجلسة: تحديث الصلاحيات + `useRouteAccessRevalidation` يخرج من المسار الممنوع؛ كاش الجلسة يُصفَّر عند انتهاء الجلسة/تبديل الفرع.

## ملفات المسؤولية

| ملف | المسؤولية |
|-----|-----------|
| `Application/Common/AdminPanel/AdminPanelTabs.cs` | كتالوج المفاتيح والحزم |
| `Get/PatchUserTabAccess*` | عقود GET/PATCH |
| `Infrastructure/Services/AuthService.cs` | تجميع `PanelTab` للعميل |
| `modules/Core/navigation/panelTabAccess.ts` | خرائط المفاتيح + أوصاف الأثر |
| `modules/User/.../PowersForm/*` | محرر المسودة/409/Neumorphism |
| `useUserTabAccessQuery` / `usePatchTabAccessMutation` | عميل HTTP |
| `useSideBar.ts` / `resolveRouteAccess.ts` | حراسة التنقل |
| `useManagementAccess.ts` / `Management/router.ts` | تبويبات لوحة الإدارة |
| `UserCreateForm.vue` / `UserTabs.vue` | إنشاء + تعديل بنفس المحرر |

## إضافة تبويب جديد

1. أضف تعريفًا في `AdminPanelTabs.BuildAll` بمفتاح فريد و`Ready` و`GrantedActions`.
2. ازرع الدور في `RoleDefaultMatrix` إن لزم.
3. سجّل القسم في `navigation.registry.ts` بنفس `id`/`tabKey`.
4. أضف المفتاح إلى `SIDEBAR_ID_TO_PANEL_TAB` أو `MANAGEMENT_TAB_TO_PANEL_TAB` / حاوية `anyOf`.
5. اختياري: وصف أثر في `TAB_EFFECT_NOTES_AR`.
6. شغّل اختبار `panelTabAccess.test.ts` لكشف المكرر/الناقص.

## إثباتات آلية (محلي)

| فحص | النتيجة |
|-----|---------|
| `tabAccessFormState.test.ts` | نجح |
| `panelTabAccess.test.ts` | نجح |
| `vue-tsc --noEmit` | نجح |
| `vite build` | نجح |
| نشر إنتاج | لم يُنفَّذ |

## إثبات المتصفح على المعزولة

يُكمَّل يدويًا على `qalaat_22_23_iso` بعد تشغيل API + لوحة محليًا:

1. فتح تبويب لحساب محدود → حفظ → دخول الحساب → عملية مسموحة.
2. قفل التبويب والحساب داخل الصفحة → URL وAPI يُرفضان؛ لا ومضة بيانات قديمة.
3. تقديم استثناء من غير المسند مقابل البت من `sr.exceptions`.
4. إنشاء حساب بقالب دور + تخصيص؛ فشل شبكة يبقي المسودة؛ 409 يعرض التعارض.
5. لقطات: محرر هاتف/سطح مكتب، حساب مقفول، تعارض حفظ.

حالة الإثبات الحي: **معلّق على بيئة iso** (لم تُشغَّل في هذه الجلسة). المنفّذ والمختبر آليًا أعلاه.
