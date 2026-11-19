# متغيرات الزبون (Customer) — مرجع Migrations

تاريخ الجرد: 2026-11-18  
مصادر: `Domain/Entities/CustomerAggregate/*` + `CustomerConfiguration` + جدول الإنتاج `Customers` على `qalaat-db`.

---

## 1) خريطة سريعة: شاشة التعديل → العمود

| واجهة اللوحة (عربي) | خاصية الدومين | عمود PostgreSQL | النوع |
|---|---|---|---|
| الأسم الثلاثي | `FullName` | `FullName` | `varchar(50)` NOT NULL |
| أسم الأم | `MotherName` | `MotherName` | `varchar(50)` NOT NULL |
| رقم الهوية | `NationalCode` | `NationalCode` | `varchar(50)` NOT NULL |
| تاريخ الميلاد | `BirthDate` | `BirthDate` | `date` NOT NULL |
| رقم الهاتف | `PhoneNumber` | `PhoneNumber` | `varchar(30)` NOT NULL |
| رقم الهاتف وتساب | `WhatsAppPhoneNumber` | `WhatsAppPhoneNumber` | `varchar(30)` NOT NULL |
| أسم المتجر | `Business.Name` | `Business_Name` | `varchar(100)` NOT NULL |
| عنوان المتجر | `Business.Address` | `Business_Address` | `varchar(300)` NOT NULL |
| أقرب نقطة دالة | `Business.NearestKnownLocation` | `Business_NearestKnownLocation` | `varchar(200)` NOT NULL |
| (GPS — غير ظاهر دائماً) | `Business.GpsLatitude` | `Business_GpsLatitude` | `float8` NULL |
| (GPS) | `Business.GpsLongitude` | `Business_GpsLongitude` | `float8` NULL |
| التقييم (موظف) | ليس عمود على `Customers` | جدول `CustomerStaffRatings` | انظر §4 |

ملاحظات الواجهة:
- حقل «التقييم: سيء» في لقطة الشاشة ليس عموداً على `Customers`؛ مصدره تقييمات الموظفين (`CustomerStaffRatings`) أو منطق عرض منفصل.
- `Business` مركّب (EF Complex Property) — أعمدته على نفس جدول `Customers` ببادئة `Business_`.

---

## 2) جدول `Customers` — الحالة الفعلية في الإنتاج

كل الأعمدة الموجودة الآن:

| العمود | النوع | Nullable | Default | ملاحظات |
|---|---|---|---|---|
| `Id` | `int4` identity | NO | | PK |
| `FullName` | `varchar(50)` | NO | | |
| `MotherName` | `varchar(50)` | NO | | فارغ مسموح منطقياً عبر قيم نصية فارغة من الواجهة |
| `NationalCode` | `varchar(50)` | NO | | |
| `BirthDate` | `date` | NO | | |
| `PhoneNumber` | `varchar(30)` | NO | | |
| `WhatsAppPhoneNumber` | `varchar(30)` | NO | | |
| `Business_Name` | `varchar(100)` | NO | | |
| `Business_Address` | `varchar(300)` | NO | | |
| `Business_NearestKnownLocation` | `varchar(200)` | NO | | |
| `Business_GpsLatitude` | `float8` | YES | | من مسؤول اللجنة؛ منفصل عن `Order.Location` |
| `Business_GpsLongitude` | `float8` | YES | | |
| `BranchId` | `int4` | NO | `0` | FK → `Branches` |
| `IsLegal` | `bool` | NO | `false` | قانونية الزبون (قديم: `Customers.IsLegal`) |
| `IsFakeSale` | `bool` | NO | `false` | بيع وهمي |
| `ImportKey` | `varchar(120)` | YES | | مثال: `najaf\|Customers\|{id}` — **موجود في الإنتاج** |
| `CreatedAt` | `timestamptz` | YES | | من `AuditableEntity` |
| `CreatedBy` | `int4` | NO | | |
| `LastUpdatedAt` | `timestamptz` | YES | | |
| `LastUpdatedBy` | `int4` | NO | | |

### فهارس الإنتاج

| الاسم | التعريف |
|---|---|
| `PK_Customers` | UNIQUE (`Id`) |
| `IX_Customers_BranchId` | INDEX (`BranchId`) |
| `UX_Customers_ImportKey` | UNIQUE (`ImportKey`) WHERE `ImportKey IS NOT NULL` |

علاقات:
- `Attachments.CustomerId` → Cascade  
- `Orders.CustomerId` → Restrict  
- `BranchId` → `Branches` Restrict  

---

## 3) كيان الدومين `Customer` (ملخص الخصائص)

```
AuditableEntity
  Id, CreatedAt, CreatedBy, LastUpdatedAt, LastUpdatedBy

Customer
  FullName, MotherName, NationalCode, BirthDate
  PhoneNumber, WhatsAppPhoneNumber
  Business (owned/complex)
  BranchId
  IsLegal, IsFakeSale
  ImportKey?                          // مُرحَّل + موجود بقاعدة الإنتاج
  SellAmountDiscount                  // في الكود — يحتاج Migration (انظر §5)
  DailyInstallmentDiscount            // في الكود — يحتاج Migration (انظر §5)

Navigation
  Branch?, Attachments, Orders
```

### `Business` (owned)

| خاصية | عمود | مطلوب |
|---|---|---|
| `Name` | `Business_Name` | نعم |
| `Address` | `Business_Address` | نعم |
| `NearestKnownLocation` | `Business_NearestKnownLocation` | نعم |
| `GpsLatitude` | `Business_GpsLatitude` | لا |
| `GpsLongitude` | `Business_GpsLongitude` | لا |

قواعد GPS:
- `HasGps` = إحداثيات غير null وغير NaN وغير (0,0) تقريباً.
- `SetGps` يرفض lat خارج [-90,90] و lng خارج [-180,180].

---

## 4) جداول مرتبطة بالزبون (ليست أعمدة على `Customers`)

| جدول | الغرض |
|---|---|
| `CustomerStaffRatings` | تقييم موظف للزبون (1–10) حسب الدور: `Delegate` / `CommitteeField` / `SalesEmployee` |
| `CustomerNotes` | ملاحظات |
| `CustomerDecisions` | قرارات قانونية/إدارية |
| `CustomerWaiverRequests` | طلبات تنازل |
| `FollowerCustomerRatingSnapshots` | لقطات تقييم المتابع |

`CustomerStaffRating` حقول أساسية: `CustomerId`, `Role`, `Score`, `Note?`, `RatedByUserId`, `RatedByName?`, `RatedAtUtc`, `OrderId?`.

---

## 5) فجوة الكود ↔ قاعدة البيانات (مهم قبل أي Migration)

| الحقل في الكود | في Snapshot EF | في إنتاج `Customers` | الإجراء |
|---|---|---|---|
| `ImportKey` | نعم | نعم (+ فهرس فريد) | لا تعيد إضافته — موجود عبر `20261117192414_SafesRebuild` |
| `SellAmountDiscount` | **لا** | **لا** | **أضف Migration جديدة** |
| `DailyInstallmentDiscount` | **لا** | **لا** | **أضف Migration جديدة** |
| `Business_Gps*` | نعم | نعم | لا شيء |
| `IsLegal` / `IsFakeSale` | نعم | نعم | لا شيء |

سبب فشل نشر سابق: `PendingModelChangesWarning` عندما يُضاف حقل في الـ Entity/Configuration بدون Migration مطابقة للـ Snapshot.

### Migration المقترحة للخصومات فقط

الاسم المقترح: `AddCustomerDiscounts`

```csharp
migrationBuilder.AddColumn<double>(
    name: "SellAmountDiscount",
    table: "Customers",
    type: "double precision",
    nullable: false,
    defaultValue: 0.0);

migrationBuilder.AddColumn<double>(
    name: "DailyInstallmentDiscount",
    table: "Customers",
    type: "double precision",
    nullable: false,
    defaultValue: 0.0);
```

قواعد العمل (من الدومين):
- الموجب = خصم، السالب = إضافة على السعر/القسط.
- صفر = لا خصم.
- تُضبط من لوحة الإدارة فقط (`SetDiscounts`) — مسار التطبيق لا يمسّها.

أمر التوليد (من مجلد `api-main/api-main`):

```bash
dotnet ef migrations add AddCustomerDiscounts \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj \
  --context ApplicationDbContext
```

ثم راجع الـ Designer/Snapshot أن الخصمين دخلا فعلاً، وأن `ImportKey` لم يُحذف/يُعاد.

---

## 6) `ImportKey` — عقد الترحيل

| البند | القيمة |
|---|---|
| الصيغة | `{source}\|Customers\|{sourceId}` مثال: `najaf\|Customers\|123` |
| الغرض | هوية ثابتة للاستيراد — **لا دمج بالاسم/الهاتف** |
| الفهرس | `UX_Customers_ImportKey` فريد جزئي حيث NOT NULL |
| نجف حالياً | كل زبائن الفرع 2 لديهم `ImportKey` (5859/5859) |

لا تغيّر طول العمود عن `120` دون مراجعة المفاتيح الموجودة.

---

## 7) Checklist قبل تطبيق Migration على الإنتاج

1. `dotnet ef migrations has-pending-model-changes` (أو بناء API ومراقبة `PendingModelChangesWarning`).
2. تأكد أن Migration الجديدة تضيف **فقط** الخصمين إن كان الهدف ذلك — لا تلمس `ImportKey` إن كان موجوداً.
3. طبّق على staging ثم production.
4. تحقق:
   ```sql
   SELECT column_name FROM information_schema.columns
   WHERE table_name='Customers'
   ORDER BY ordinal_position;
   ```
5. القيم الافتراضية للصفوف القديمة = `0` لكلا الخصمين.

---

## 8) مراجع الملفات

| ملف | دور |
|---|---|
| `Domain/Entities/CustomerAggregate/Customer.cs` | الكيان |
| `Domain/Entities/CustomerAggregate/Business.cs` | المركّب التجاري + GPS |
| `Domain/Entities/CustomerAggregate/CustomerStaffRating.cs` | تقييم الموظف |
| `Infrastructure/.../Customer/CustomerConfiguration.cs` | أطوال/فهارس/Defaults |
| `Migrations/20261117192414_SafesRebuild.cs` | أضاف `ImportKey` + `UX_Customers_ImportKey` |
| `Migrations/20261116180312_CustomerGpsAndStaffRatings*.cs` | GPS + `CustomerStaffRatings` (سلسلة سابقة) |
