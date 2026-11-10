# 11 — مصفوفة قواعد الجباية

إثبات: `FollowerCollectionEnabled` على القائمة (Delegates)؛ `SetFollowerCollectionEnabledAsync`؛ بوابة `CustomersPaymentsRequestsRepository` + رسالة قفل تسجيل دخول المندوب.

| القاعدة | الدليل | المقابل بالجديد |
|---|---|---|
| التمكين على **القائمة** | Delegates.FollowerCollectionEnabled | OrderList.FollowerCollectionEnabled |
| من يشغّل؟ | لوحة المدير/المراقب عبر ops | FollowerCollection.ManageEnabled |
| أثر التفعيل | يمنع جباية **المندوب** على القائمة؛ يوم الجباية للمتابع | رفض CreateInstallmentPayment إن الفاعل=Mandob والقائمة مفعّلة |
| AssignedAtUtc | وقت التفعيل | FollowerCollectionAssignedAtUtc عند enabled=true |
| عند الإيقاف | لا يحذف دفعات تاريخية | تحديث العلم فقط |
| نقل التكليف | العلم على القائمة يبقى مع القائمة | يبقى على OrderList |
| دفعات العرض | CustomersPaymentsRequest (متابع) + مندوب أمس | InstallmentPayments على قوائم النطاق مع تصنيف المصدر عبر CreatedBy |
| لا 1448 | تحويل عملة قديم | مبالغ النظام الحالي كما هي |
| قراءة التاريخ | تبقى بعد الإيقاف | نعم |
| إنشاء دفعة من اللوحة | غير موجود | لا زر |
