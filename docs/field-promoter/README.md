# المروّج الميداني

بديل تطبيق المسوّقين الميداني القديم (`PromoterApps/FieldPromoterApp` في `0pprp/qalatsystemfromzaid`) داخل النظام الجديد: نفس المنطق، بواجهة جديدة، وبلا عيوب القديم.

## أين يوجد كل شيء
| الجزء | المكان |
|---|---|
| الكيانات | `api-main/api-main/src/Domain/Entities/FieldPromoterAggregate/FieldPromoterEntities.cs` |
| القواعد المشتركة (الاسم، الهاتف، حدود العراق، الفترات، الوقوف، تنظيف الصور) | `src/Application/FieldPromoters/Common/FieldPromoterRules.cs` |
| الوصول والحقائق اليومية والأهداف | `src/Application/FieldPromoters/Common/FieldPromoterAccess.cs` |
| الدورة الدورية (إغلاق الدوام، التنبيهات، تجميد الأيام، الاحتفاظ) | `src/Application/FieldPromoters/Common/FieldPromoterMaintenance.cs` + `src/Infrastructure/Services/FieldPromoterWorker.cs` (كل دقيقتين) |
| endpoints التطبيق | `src/Application/FieldPromoters/App/FieldPromoterAppHandlers.cs` ← `Api/Controllers/V1/AppArea/FieldPromoterController.cs` |
| endpoints اللوحة | `src/Application/FieldPromoters/Admin/FieldPromoterAdminHandlers.cs` ← `Api/Controllers/V1/AdminArea/FieldPromotersController.cs` |
| الـ migration | `Infrastructure/Persistence/Migrations/20261120221720_FieldPromoters.cs` (6 جداول جديدة فقط) |
| الاختبارات | `tests/Application.Tests/FieldPromoters/FieldPromoterTests.cs` |
| عقد الـ API | [api-contract.md](api-contract.md) |
| القواعد بالتفصيل | [rules.md](rules.md) |
| ما أُصلح من القديم | [differences.md](differences.md) |
| التطبيق | `field_promoter_app/` (مفتاح التحديث `field-promoter`) |
| اللوحة | قسم «المروّجون الميدانيون» في `panel-admin-main` |

## إنشاء مروّج
من اللوحة: «المروّجون» ← إضافة. الحساب مستخدم عادي بدور «مروّج ميداني» (`FieldPromoter`) وفروع يرسل لها الطلبات. كلمة المرور 8 أحرف على الأقل.

## الدوام والتتبّع
- الدوام يبدأ تلقائياً مع أول طلب أو أول محل، أو يدوياً من التطبيق.
- يُغلق تلقائياً بنهاية يوم العراق عند آخر نقطة موقع.
- نقاط الموقع تُحذف تلقائياً بعد 90 يوماً (`FieldPromoterRules.LocationRetentionDays`).
