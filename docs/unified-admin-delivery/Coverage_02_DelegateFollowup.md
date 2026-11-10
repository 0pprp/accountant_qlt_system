# Coverage_02 — استكمال متابعة المندوبين

تاريخ: 2026-11-08  
إثبات: مطابقة مصدرية (FE/BE_Company) + تحقق حي معزول على الجديد. لا مقارنة حية بالقديم. لا نشر.

## هوية القائمة

| قديم | جديد | ملاحظة |
|---|---|---|
| `Delegates.DelegateID` AS ListId | `OrderList.Id` | عقد المعمارية الجديدة؛ ليس تطابق أرقام |
| مندوب | `OrderList.MandobId` → User | |
| متابع / مسؤول لجنة | `OrderList.MotabaId` → User | |
| زبائن القائمة | Orders على OrderListId + Customer | |
| تسديدات | InstallmentPayments على Orders المعتمدة | |
| `DelegateState=1` | لا مقابل soft-active على OrderList | **فجوة موثّقة**: النشاط = آخر تسديد ≤40ي + رصيد مفتوح فقط |

## مصفوفة الفجوات

| الميزة/الحقل/الزر | دليل المصدر | المقابل الجديد | الحالة قبل 02 | التعديل المطلوب | اختبار القبول |
|---|---|---|---|---|---|
| قوائم نشطة | Rules MaxDays=40 + DelegateState | ListActiveListsAsync | منفّذ بلا State | توثيق فجوة State؛ الإبقاء على 40ي | قوائم ضمن/خارج 40ي |
| IsLegal | Customers.IsLegal BIT | Customer.IsLegal | غير موجود | إضافة عمود + migration | عدّ قانونية وفلتر |
| القسط المطلوب | Sum AmountDaySales حيث Remaining>0 و !IsLegal | SumRequiredDueInstallment | محسوب للكل + ملاحظة | استثناء القانونية؛ لا صفر بديل إن تعطّل | عينة يدوية |
| تقييم عامل اللوحة | PK (ListId,RatingDate) 0–10 | DirectorBoardOperatorListRatings UX | منفّذ | تأكيد uniqueness + تعارض DB | upsert متزامن |
| تقييم متابع/إشراف/مندوب مبيعات | 3 جداول يومية | كيانات جديدة على OrderListId | Available=false | جداول + قراءة على اللوحة | قراءة فارغة/موجودة |
| جدول زبائن مضمّن | board filter | موجود | منفّذ | إضافة isLegal + فلتر قانونية | فلتر legal |
| صفحة تفاصيل كاملة | director-delegate-followup-customers | ناقص | صفحة جديدة + return context | حقول/فرز/تسديدات/Excel | استعادة سياق |
| تسديدات الزبون GET | CustomersPayments_GetByCustomerID | InstallmentPayments scoped | ناقص | endpoint قراءة | فرع غير مسموح 403 |
| إنشاء تسديد | API موجود؛ **الواجهتان لا تستدعيانه** | — | — | لا UI إنشاء في 02 (مطابقة شاشات) | موثّق |
| مجموع فترة القائمة | stats/list-payments-sum (يتيم في FE) | اختياري | — | إن نُفّذ: نفس نطاق الفرع | — |

## مؤشرات — معادلات

| المؤشر | المعادلة | المجموعة | المستثنى | الفترة | الوحدة | الجديد |
|---|---|---|---|---|---|---|
| القسط المطلوب | Σ AmountDaySales | Remaining>0 | IsLegal | asOf | IQD | بعد IsLegal |
| غير المصفرين فئات | Count by rating label | Remaining>0 و !IsLegal للتسمية؛ القانونية عدّ منفصل | — | asOf | عدد | |
| باقي &lt;100k / 100–200k | CountRemainingBands | Remaining>0 (يشمل قانونية) | لا | asOf | عدد | كما المصدر |
| مبيعات بلا جباية ≥3ي | لا تسديد + أيام≥3 | Remaining>0 و !IsLegal | قانونية | asOf | عدد | |
| واردات أمس/جمعة/30ي | Σ payments | طلبات القائمة | — | يوم/أيام | IQD | موجود |

## غير محسوم المتبقي
- مكافئ `DelegateState` على OrderList.
- كتّاب تقييمات اللجان (تطبيقات أخرى) — هذه الدفعة قراءة فقط.
- إنشاء تسديد من شاشة المتابعة (غير مستخدم في مرجع الواجهة).


## حالة الإغلاق (بعد التنفيذ)

| البند | الحالة |
|---|---|
| IsLegal + القسط المطلوب الصحيح | منفّذ ومختبر |
| تقييمات اللجان (جداول + قراءة) | منفّذ ومختبر (فارغ حتى يكتب المنتجون) |
| صفحة تفاصيل الزبائن + تسديدات GET | منفّذ |
| إنشاء تسديد من الشاشة | N/A — غير مستخدم في مرجع الواجهة |
| DelegateState | غير محسوم |
| لقطات حية | معلّق |
