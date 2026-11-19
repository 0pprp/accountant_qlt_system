# بنية حقول الزبون — Customers

مرجع كامل لجدول `Customers` كما هو في الكود بعد إضافة حقلَي الخصم.
المصدر الوحيد للحقيقة هو [Customer.cs](../../api-main/api-main/src/Domain/Entities/CustomerAggregate/Customer.cs)
و[CustomerConfiguration.cs](../../api-main/api-main/src/Infrastructure/Persistence/EntityTypeConfigurations/Customer/CustomerConfiguration.cs).

## الأعمدة

| العمود | نوع C# | نوع PostgreSQL | Null | الطول | الافتراضي | ماذا يعني |
|---|---|---|---|---|---|---|
| `Id` | `int` | `integer` | NO | — | identity | المفتاح الأساسي |
| `FullName` | `string` | `character varying` | NO | 50 | — | الاسم الثلاثي |
| `MotherName` | `string` | `character varying` | NO | 50 | — | اسم الأم |
| `NationalCode` | `string` | `character varying` | NO | 50 | — | رقم الهوية |
| `BirthDate` | `DateOnly` | `date` | NO | — | — | تاريخ الميلاد، يُتحقَّق ≥ ١٨ سنة |
| `PhoneNumber` | `string` | `character varying` | NO | 30 | — | الهاتف، ١١ رقماً في التحقق |
| `WhatsAppPhoneNumber` | `string` | `character varying` | NO | 30 | — | هاتف واتساب |
| `Business_Name` | `string` | `character varying` | NO | 100 | — | اسم المتجر — ComplexProperty |
| `Business_Address` | `string` | `character varying` | NO | 300 | — | عنوان المتجر |
| `Business_NearestKnownLocation` | `string` | `character varying` | NO | 200 | — | أقرب نقطة دالة |
| `Business_GpsLatitude` | `double?` | `double precision` | YES | — | — | خط العرض |
| `Business_GpsLongitude` | `double?` | `double precision` | YES | — | — | خط الطول |
| `BranchId` | `int` | `integer` | NO | — | 0 | الفرع — FK إلى `Branches`، حذف مقيَّد |
| `IsLegal` | `bool` | `boolean` | NO | — | false | محوَّل قانونياً |
| `IsFakeSale` | `bool` | `boolean` | NO | — | false | بيع وهمي |
| **`SellAmountDiscount`** | **`double`** | **`double precision`** | **NO** | — | **0** | **خصم سعر البيع — يقبل السالب والموجب** |
| **`DailyInstallmentDiscount`** | **`double`** | **`double precision`** | **NO** | — | **0** | **خصم القسط اليومي — يقبل السالب والموجب** |
| `ImportKey` | `string?` | `character varying` | YES | 120 | — | مفتاح استيراد، فهرس فريد مُرشَّح |
| `CreatedAt` | `DateTimeOffset?` | `timestamptz` | YES | — | — | من `AuditableEntity` |
| `CreatedBy` | `int` | `integer` | NO | — | — | من `AuditableEntity` |
| `LastUpdatedAt` | `DateTimeOffset?` | `timestamptz` | YES | — | — | من `AuditableEntity` |
| `LastUpdatedBy` | `int` | `integer` | NO | — | — | من `AuditableEntity` |

## الفهارس والعلاقات

| الاسم | النوع | التفصيل |
|---|---|---|
| `PK_Customers` | مفتاح أساسي | `Id` |
| `UX_Customers_ImportKey` | فريد مُرشَّح | `ImportKey` حيث `"ImportKey" IS NOT NULL` |
| `FK_Customers_Branches_BranchId` | أجنبي | → `Branches.Id`، `OnDelete: Restrict` |
| `Attachments.CustomerId` | أجنبي وارد | `OnDelete: Cascade` — حذف الزبون يحذف مرفقاته |
| `Orders.CustomerId` | أجنبي وارد | `OnDelete: Restrict` — زبون له طلبات لا يُحذف |

## حقلا الخصم — السلوك

- **النوع:** `double precision NOT NULL DEFAULT 0`. الصفوف القائمة كلها تصير صفراً بلا تدخّل.
- **الإشارة:** السالب والموجب مقبولان. الموجب خصم، والسالب إضافة على المبلغ.
- **من يكتبهما:** لوحة الإدارة وحدها عبر `PUT /api/v1/admin/Customers/{id}`.
  مسار التطبيق (`AppArea/CustomersController`) لا يمسّهما إطلاقاً — `Customer.Update(...)`
  لا تشملهما، والضبط يتم بـ `Customer.SetDiscounts(...)` التي يناديها معالج الإدارة فقط.
  فتعديل الزبون من تطبيق المندوب لا يُصفّر خصماً كتبته أنت.
- **التحقق:** `double.IsFinite` فقط — لا حدّ أعلى ولا أدنى ولا اشتراط مضاعفات الألف.
- **لا يُحتسب منهما شيء بعد.** الحقلان يُخزَّنان ويُقرآن ويُعدَّلان، ولا يدخلان في أي
  حساب لسعر البيع أو القسط أو المتبقّي حتى الآن. ربطهما بالحسابات قرار منفصل.

## المسارات المتأثرة

| الملف | الطبقة | ماذا تغيّر |
|---|---|---|
| [Customer.cs](../../api-main/api-main/src/Domain/Entities/CustomerAggregate/Customer.cs) | Domain | الخاصيتان + `SetDiscounts` |
| [CustomerConfiguration.cs](../../api-main/api-main/src/Infrastructure/Persistence/EntityTypeConfigurations/Customer/CustomerConfiguration.cs) | Infrastructure | عمودان بافتراضي صفر |
| [UpdateCustomerCommand.cs](../../api-main/api-main/src/Application/Customers/Commands/Admin/Update/UpdateCustomerCommand.cs) | Application | الحقلان في الأمر والـDto |
| [UpdateCustomerCommandValidator.cs](../../api-main/api-main/src/Application/Customers/Commands/Admin/Update/UpdateCustomerCommandValidator.cs) | Application | `IsFinite` برسالة عربية |
| [UpdateCustomerHandler.cs](../../api-main/api-main/src/Application/Customers/Commands/Admin/Update/UpdateCustomerHandler.cs) | Application | نداء `SetDiscounts` |
| [GetCustomerByIdResponse.cs](../../api-main/api-main/src/Application/Customers/Queries/Admin/GetById/GetCustomerByIdResponse.cs) | Application | الحقلان في الاستجابة |
| [GetCustomerByIdHandler.cs](../../api-main/api-main/src/Application/Customers/Queries/Admin/GetById/GetCustomerByIdHandler.cs) | Application | قراءة الحقلين |
| [AdminArea/CustomersController.cs](../../api-main/api-main/src/Api/Controllers/V1/AdminArea/CustomersController.cs) | Api | تمرير الحقلين من الـDto |
| [CustomerForm.vue](../../panel-admin-main/panel-admin-main/src/modules/Customer/components/CustomerForm/CustomerForm.vue) | Panel | قسم «الخصومات» وحقلاه |
| [CustomerUpsertView.vue](../../panel-admin-main/panel-admin-main/src/modules/Customer/views/CustomerUpsertView.vue) | Panel | تعبئة القيم الأولية |
| [types/model/index.ts](../../panel-admin-main/panel-admin-main/src/modules/Customer/types/model/index.ts) · [types/api/index.ts](../../panel-admin-main/panel-admin-main/src/modules/Customer/types/api/index.ts) | Panel | الأنواع |
| [localization/ar.json](../../panel-admin-main/panel-admin-main/src/modules/Customer/localization/ar.json) | Panel | التسميات العربية |

## الهجرة

لم أولّد ملف الهجرة — `AGENTS.md` يمنعني. نفّذها أنت:

```bash
cd api-main/api-main
dotnet ef migrations add CustomerDiscounts \
  --project src/Infrastructure \
  --startup-project src/Api
```

**اقرأ الملف المُولَّد قبل تطبيقه.** المتوقّع فيه عمودان لا أكثر:

```csharp
migrationBuilder.AddColumn<double>(
    name: "SellAmountDiscount", table: "Customers",
    type: "double precision", nullable: false, defaultValue: 0.0);

migrationBuilder.AddColumn<double>(
    name: "DailyInstallmentDiscount", table: "Customers",
    type: "double precision", nullable: false, defaultValue: 0.0);
```

إن ظهر فيه أي شيء غير هذين العمودين — حذف جدول، تغيير عمود قائم، إعادة إنشاء فهرس —
**فلا تطبّقه**، لأن معناه أن الـsnapshot غير متزامن مع قاعدة الإنتاج.

ثم التطبيق:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

لا يحتاج ترحيل بيانات: `defaultValue: 0.0` يملأ كل الصفوف القائمة.
