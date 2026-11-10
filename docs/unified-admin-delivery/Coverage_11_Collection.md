# Coverage 11 — الجباية

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم (علم القائمة + قراءة حزمة + PATCH تمكين + بوابة دفع المندوب) |
| مختبر وحدويًا | نعم (قواعد التمكين + صلاحيات مستقلة + vitest) |
| مثبت حيًا | ناقص — لا عزل DB مطبَّق ولا تحقق جهاز في هذه الجلسة |
| ناقص | قفل تسجيل دخول المندوب كما في المصدر القديم (بوابة الدفع منفَّذة؛ login-lock اختياري لم يُنقل حرفيًا) |
| غير محسوم | لا — التمكين على القائمة مثبت |

## المصدر المثبت
- `Delegates.FollowerCollectionEnabled` + `SetFollowerCollectionEnabledAsync`
- بوابة `CustomersPaymentsRequestsRepository` تمنع جباية المندوب عند التفعيل
- العرض: قوائم + مدفوعات طلبات المتابع + مجموع مندوب أمس

## العقود
- `GET /api/v1/admin/FollowerWork/{followerId}/collection` — `FollowerCollection.Read`
- `PATCH /api/v1/admin/OrderLists/{listId}/follower-collection` — `FollowerCollection.ManageEnabled`
  - Body: `{ enabled, expectedVersion }` — لا branchId/followerId
- بوابة: `CreateInstallmentPayment` (App) يرفض إن الفاعل=Mandob والقائمة مفعّلة (داخل معاملة)

## قواعد
- الإجمالي المفلتر من DB وليس مجموع الصفحة فقط.
- لا *1448 — مبالغ IQD كما في النظام الجديد.
- إعادة نفس الحالة بلا أثر مكرر ولا رفع نسخة.
- 409 عند تعارض النسخة.
- الإيقاف لا يحذف دفعات تاريخية.
