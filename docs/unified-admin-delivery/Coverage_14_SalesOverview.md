# Coverage 14 — نظرة عامة للمبيعات

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم — GET SalesOverview + صفحة `/sale/overview` + صلاحية SalesOverview.Read |
| مختبر وحدويًا | نعم (`SalesWorkDayIraqTests` + `SalesOverviewMetricKeysTests` + nav) |
| مثبت حيًا | ناقص — لا API-live / Browser-live / Device في هذه الجلسة |
| ناقص | لقطات 360/768/1440؛ حسابات قارئ-فقط مقابل فرع محدود على بيئة معزولة |
| غير محسوم | لا |

## المؤشرات (من المصدر فقط)
1. موظفين في الدوام — SalesShift نشط لـ Mandob
2. بدون دوام
3. الموقع المباشر — عدد Live فقط بلا إحداثيات
4. المبيعات اليوم — Order Approved + SaleDate يوم العمل بغداد
5. الطلبات المعلقة — SalesRequest Pending غير محذوف
6. طلبات جديدة — SalesRequest New غير محذوف

## عقد
- `GET /api/v1/admin/SalesOverview?branchId=`
- Permission: `SalesOverview.Read`
- الروابط مشروطة بصلاحية الوجهة (لا وصول عبر رابط مباشر بلا إذن)

## نواقص سابقة محفوظة
- 10/11/12/13 كما في Coverage الخاصة — لم تُغيَّر إلى مكتمل
