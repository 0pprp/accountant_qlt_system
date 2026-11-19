# تصنيف جداول مصدر النجف

أساس الأعداد: [Najaf-Source-Inventory.md](Najaf-Source-Inventory.md) (تقدير partitions — تُدقَّق من snapshot).  
التصنيف: **منقول** | **مرجعي** | **خارج النطاق**.

## منقول (بيانات فرع النجف)

| جدول | سبب |
|---|---|
| Customers | بروفايلات الزبائن |
| CustomersSales | المبيعات |
| SelectItemsSales | بنود المبيعات |
| CustomersPayments | وصولات التحصيل |
| AddToBox | قيود الصندوق المرتبطة |
| Boxes / Delegates / Groups / Cities | قاصات وقوائم/مندوبين ونطاق المدينة |
| SelectDelegate / UsersSelectedCities | تكليفات (بدون جلسات) |
| SalesCustomerDocuments / SalesDocuments / Documents / LegalFollowupCaseDocuments | روابط مستندات |
| CustomerShopFollowerCapture / CustomerShopFollowerPhotos / CustomerLocationEstimates | GPS فعّال |
| LegalFollowupCases / LegalConversionRequests | قانونية |
| CustomerNotes / SalesCustomerNotes | ملاحظات |
| WithdrawalFromBox / TransferBoxs | حركات قاصة تاريخية |
| WithdrawalStores / AddToStores / SelectItems* / Buys / BuysItems / Items / Stores / BarcodeItems | مخزن/مشتريات |
| SalesRequests / SalesRequestHistory (إن لزم للتاريخ) | طلبات قديمة للقراءة |
| DollarAmount / CompanyInformation | إعدادات فرع مرجعية للقراءة |

## مرجعي مشترك (يُطابق أو يُعاد بناؤه في الهدف)

| جدول | سبب |
|---|---|
| Permissions / PermissionsTypes / SetPermissions* | لا تُنقل صلاحيات Admin؛ تُعاد وفق نموذج Panel.Tab |
| Materials / Products | كتالوج قد يُدمج مع منتجات القلعة |
| Employees / Users (حسابات تشغيلية غير Admin) | يُنشأ حسابات مندوبين جديدة بمفتاح استيراد؛ لا دمج ahmed |

## خارج النطاق

| جدول | سبب |
|---|---|
| RefreshTokens / WorkSessions | جلسات |
| Notifications / DecisionNotifications / Broadcasts / PayoutMessages | لا إعادة إشعار |
| AuditLog / SalesTrackingEvents / FollowerTrackingEvents / LocationPoints / SalesLocationPoints | حجم تتبع؛ اختياري لاحقًا |
| __Migrations / __CustomerAppMigrations | هجرة قديمة |
| CrmSalesAssignCursor / SalesFilter* | تشغيل وقتي |
| DelegateDevicePaymentAudit | تدقيق جهاز؛ ليس دين زبون |

## فجوات هدف القلعة (عولجت للـsandbox)

1. `InstallmentPayment.OrderId` اختياري + `CustomerId` للوصل على الحساب — Domain و`tools/add_import_keys.sql`.
2. `ImportKey` على Customer / Order / InstallmentPayment.
3. المتبقي المصدر: `ROUND(..., -3)` عبر `CustomerIDPayment` — [BALANCE_FORMULA.md](BALANCE_FORMULA.md).
4. خريطة الأعمدة: [SCHEMA_MAP.md](SCHEMA_MAP.md).
