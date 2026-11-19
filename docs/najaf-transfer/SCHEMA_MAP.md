# خريطة مصدر → هدف (فرع النجف)

مفتاح الاستيراد الثابت: `najaf|{sourceTable}|{sourceId}` — ليس الاسم/الهاتف.  
لا دمج دخول؛ لا نقل RefreshTokens؛ الإبقاء على `ahmed` / Admin.

## جداول أساسية

| مصدر (MSSQL) | هدف (Postgres / Domain) | ImportKey | ملاحظات |
|---|---|---|---|
| Customers | Customer | `najaf\|Customers\|{Id}` | IsLegal / IsFakeSale موجودة في الهدف |
| CustomersSales | Order | `najaf\|CustomersSales\|{Id}` | ترحيل تاريخي بلا خصم مخزون |
| SelectItemsSales | Order line / items | `najaf\|SelectItemsSales\|{Id}` | مربوط بالمبيع المصدر |
| CustomersPayments | InstallmentPayment (+ تخصيص) | `najaf\|CustomersPayments\|{Id}` | انظر فجوة OrderId |
| AddToBox | Cashbox / payment allocation | `najaf\|AddToBox\|{Id}` | قد يتعدد قيد لصندوق لوصل واحد |
| Groups | Customer list / route | `najaf\|Groups\|{Id}` | قوائم المندوبين |
| Delegates / Users (غير Admin) | User (مندوب جديد) | `najaf\|Users\|{Id}` | حسابات جديدة؛ لا مسّ ahmed |
| Boxes | Cashbox | `najaf\|Boxes\|{Id}` | مطابقة قاصة مستقلة |
| Items / Stores / Buys* | Inventory | `najaf\|{table}\|{Id}` | مطابقة مخزون مستقلة عن الديون |
| CustomerShopFollowerCapture | GPS capture | `najaf\|CustomerShopFollowerCapture\|{Id}` | + أرشيف وسائط |
| CustomerShopFollowerPhotos | GPS photo | `najaf\|CustomerShopFollowerPhotos\|{Id}` | FileKey/StoragePath |
| CustomerLocationEstimates | Location estimate | `najaf\|CustomerLocationEstimates\|{Id}` | |
| LegalFollowupCases / LegalConversionRequests | Legal flags / cases | `najaf\|{table}\|{Id}` | Customer.IsLegal |
| Documents / SalesDocuments / SalesCustomerDocuments | Attachment | `najaf\|{table}\|{Id}` | عدّ المفقود عبر FileKey |

## فجوات نموذج الهدف (قبل استيراد كامل)

1. **InstallmentPayment.OrderId إلزامي** بينما المصدر يربط التحصيل بالزبون (`CustomersPayments` + `AddToBox`).  
   الحل للـsandbox: عمود/حقل `CustomerId` اختياري + `OrderId` قابل للإبطال مع جدول تخصيص اختياري `PaymentAllocation(OrderId, Amount)`؛ أو وصل افتتاحي/حساب يُنشأ كمبيع افتتاحي واحد لكل زبون عند غياب المبيع.
2. **لا ImportKey** على Customer / Order (موجود فقط على SalesRequest في بعض المسارات). يلزم عمود `ImportKey` (text, unique) أو جدول `ImportMap(ImportKey, EntityType, EntityId)`.
3. **عرض المتبقي**: المصدر يستخدم غالبًا `ROUND(sales - receipts, -3)` → فرق إجمالي معروف ≈ 542 د.ع بين الخام والمعروض.
4. **معامل 1448**: تحويل دولار→دينار في سكربتات المصدر؛ يُثبت عند قراءة أعمدة الدولار.
5. **GPS/وثائق**: أرشيف `najaf-app-media.tgz`؛ المطابقة بعدد الملفات + FileKey وليس بإعادة رفع إنتاج.

## سلوك الترحيل

- تاريخي فقط: بلا إعادة خصم مخزون / إضافة نقد / إشعارات.
- وصل متعدد قيود الصندوق لا يضاعف الدين.
- دين موجب / دائن / صافي تُبلَّغ منفصلة (لا إلغاء فروق متضادة).
