# دفعة 01 — صلاحيات اللوحة بمفتاح تبويب واحد

تاريخ: 2026-11-16  
النطاق: Backend + عقود API + هجرة معزولة + Excel  
خارج النطاق: تصميم واجهة محرّر التبويبات (دفعة 02) — **لا نشر إنتاج**

## النتيجة

| البند | القيمة |
|------|--------|
| Build Application/Api | نجح |
| Unit tests `FullyQualifiedName~Users` | **41/41 passed** |
| تبويبات في الكتالوج | ~70 تعريف (`AdminPanelTabs.All`) منها جاهزة في `ReadyTabs` و`committeeRatings` not_implemented |
| API-live iso Postgres | غير متاح في بيئة التنفيذ الحالية (`psql` غير موجود محليًا) — الهجرة مغطاة بوحدة `PanelTabMigrationTests` + endpoints Development-only |
| نشر إنتاج | **لم يُنفَّذ** |

## ما تغيّر

1. كتالوج مركزي: `Application/Common/AdminPanel/AdminPanelTabs.cs` — مفاتيح `Panel.Tab.{tabKey}`.
2. البذرة: `DatabaseInitializer` يزرع `AdminPanelTabs.OutsideAll`؛ `RoleDefaultMatrix` لـ Director/Monitor/SalesManager تُبذر بمفاتيح التبويب؛ Admin يحصل على كل `Panel.Tab.*`.
3. `EffectivePermissionCalculator` يوسّع التبويبات المفتوحة إلى `GrantedActions`؛ `ComputeRaw` للهجرة بدون توسيع.
4. عقود: `GET/PATCH /api/v1/admin/Users/{id}/tab-access`.
5. المسارات القديمة `permission-overrides` و`PUT permissions` ترفض مفاتيح `Panel.Tab.*` والحزم المُدارة.
6. `SalesRequestTabPermissions` يقبل `Panel.Tab.sr.*` إضافةً للصلاحيات القديمة.
7. هجرة: `PanelTabAccountMigrator` + `POST .../panel-tab-migration/dry-run|apply?confirmIsolated=true` (!Production).
8. توثيق: أوراق `Tabs`, `EndpointMap`, `RoleDefaults`, `MigrationReview`, `Acceptance` في `Unified_Admin_Permissions.xlsx`؛ تحديث `reference.json`.

## تشغيل الهجرة على iso (يدويًا)

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ConnectionStrings__Postgres='Host=localhost;Port=5432;Database=qalaat_22_23_iso;Username=postgres;Password=postgres'
$env:ASPNETCORE_URLS='http://127.0.0.1:4200'
cd api-main/api-main
dotnet run --project src/Api/Api.csproj --no-launch-profile
# بعد الدخول كـ Admin:
# POST /api/v1/admin/panel-tab-migration/dry-run
# POST /api/v1/admin/panel-tab-migration/apply?confirmIsolated=true
```

## حسابات تحتاج مراجعة

صفوف `decision=review` من dry-run (وصول جزئي سابقًا) — تُقفل افتراضيًا وتُدرج Owned/WouldGain في تقرير الهجرة. لا تُفتح تلقائيًا.

## Endpoints غير محسومة بالكامل

- مسارات reuse بنفس `Customer.Read` / `Order.Read` عبر عدة تبويبات بنفس النطاق: الاعتماد على توسيع الحزمة + فلاتر الحالة/mailbox في الـhandler (WrongSale عبر `AdminPanelTabAccess.CanAccessWrongSaleMailbox` جاهز للربط في دفعة لاحقة إن لزم).
- اشتراكات Hub تُعاد بعد إعادة الدخول (`RefreshSubscriptions`)؛ الإبطال قائم عبر `SecurityStamp`.

## دفعة 01 واجهة (مكتملة في المستودع)

انظر `PANEL_TAB_BATCH01_UI.md` — محرر التبويبات + `PanelTab` في السايدبار/الحراسة + إنشاء/تعديل.  
إثبات المتصفح على iso: معلّق. **لا نشر إنتاج.**
