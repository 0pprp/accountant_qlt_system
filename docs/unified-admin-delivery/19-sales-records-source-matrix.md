# 19 — مصفوفة مصدر المبيعات المنجزة

مصدر: `sales-manager-sales.vue` + تنقل البروفايل.
جديد: `GET /api/v1/admin/SalesRecords` + `/sale/completed` (تفاصيل عبر `SaleViewRoute`).

| الحقل/إجراء | دليل المصدر | المعنى | المقابل الجديد | الصلاحية | اختبار القبول |
|---|---|---|---|---|---|
| بيع منجز | قائمة المبيعات المكتملة | بيع معتمد | `Order.ApprovalStatus=Approved` | Order.Read | طلب Completed بلا Order ≠ صف |
| تاريخ البيع | تاريخ الإنجاز/البيع | يوم احتساب البيع | `Order.SaleDate` | Order.Read | ليس CreatedAt وحده |
| رقم البيع | id | هوية | `Order.Id` | Order.Read | — |
| الزبون | اسم/هاتف | هوية | CustomerId/Name/Phone | Order.Read؛ بروفايل منفصل | رابط البروفايل لا يتجاوز حمايته |
| البائع | employee | إسناد تاريخي | `SellerId`/`Seller.FullName` | Order.Read | يظهر بعد انتهاء التكليف |
| الفرع | branch | نطاق | `BranchId` ضمن فروع القارئ | Order.Read | فرع غير مسموح يُرفض |
| سعر البيع | finalSalePrice | إجمالي | `SellAmount` د.ع | Order.Read | — |
| العربون | downPayment | دفعة أولى | `PrepaymentAmount` | Order.Read | — |
| القسط | dailyInstallment | خطة | `DailyInstallmentAmount` | Order.Read | — |
| الواصل | مدفوعات البيع | على مستوى الصف | مجموع `InstallmentPayments` لهذا Order | Order.Read | ليس مجموع دفعات الزبون كلها |
| الباقي | sell−واصل | دين هذا البيع | `SellAmount − Receipts` | Order.Read | لا دمج ديون بيوع متعددة |
| الحالة | مكتمل/معتمد | نص+لون | ApprovalStatus=Approved → «معتمد» | Order.Read | ملغى/مرفوض خارج القائمة |
| طلب مرتبط | اختياري | تتبع | `SalesRequest.LinkedOrderId` → id | Order.Read | لا ينشئ Sale ثانيًا |
| مستندات | تفاصيل | تنزيل مخوّل | عبر تفاصيل البيع الحالية | صلاحية المرفق/البيع | لا روابط عامة ثابتة |
| تفاصيل البيع | تنقل | قراءة | `/sale/:id/view` | Order.Read | القائمة لا تتجاوز حماية التفاصيل |
| بروفايل + عودة | تنقل | سياق فلاتر | return allowlist `SaleCompletedRoute` | بروفايل | لا return خارجي تعسفي |
| تصدير | غير موجود في المرجع | — | غير منفَّذ | — | لا يغلق فجوة Excel لـ16 |
| مجاميع | إن وُجدت | نطاق الفلاتر | Totals على كل النتائج المطابقة | Order.Read | ليست مجموع الصفحة إلا إذا سُميت كذلك |
| فرز/صفحة | قائمة | ثابت | allowlist sort + page | Order.Read | لا SQL من العميل |

أهلية تنفيذ بيع جديد (تكليف) منفصلة عن حق قراءة بيع تاريخي.
