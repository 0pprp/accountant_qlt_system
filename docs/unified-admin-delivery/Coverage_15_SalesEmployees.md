# Coverage 15 — موظفو المبيعات

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم — GET list + rating-summary + صفحة `/sale/employees` + SalesEmployee.Read |
| مختبر وحدويًا | نعم (`SalesEmployeeIdentityTests` + nav ready) |
| مثبت حيًا | ناقص — لا تحقق حي في هذه الجلسة |
| ناقص | خريطة/مسار موظف المبيعات (ليست خريطة المتابع)؛ لقطات تفاعل |
| غير محسوم | هل كل مندوبي الجباية (Mandob) يجب استثناؤهم إن وُجد تمييز أدق لاحقًا — حاليًا Mandob≠Motaba كما وثّق |

## الهوية
- موظف مبيعات = دور Mandob + حساب مكتمل + فرع ضمن النطاق
- ليس: وجود حساب وحده، Hub، آخر login، GPS وحده
- SellerId على Order = هوية البيع/التقييم عند وجود مبيعات؛ القائمة تشمل بلا مبيعات

## عقد
- `GET /api/v1/admin/SalesEmployees?branchId&search&shiftState&page&pageSize&sort`
- `GET /api/v1/admin/SalesEmployees/{id}/rating-summary?branchId&bucket`
- لا CRUD حساب ولا حظر تطبيق من هذه الشاشة (ManageAppBlock عبر SalesFollowup)

## نواقص سابقة محفوظة
- 10–13 تبقى مفتوحة دون تغيير حالة القبول
