# Coverage 18 — مسارات موظفي المبيعات

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم — GET `{employeeId}/route` + `/sale/routes` + SalesRoute.Read |
| مختبر وحدويًا | نعم — حدود يوم بغداد ≠ 03:00؛ عزل صلاحية وممثل ActorKind |
| مثبت حيًا | ناقص — لا Device-verified / API-live على قاعدة معزولة بهذا التسليم |
| ناقص | إثبات مسار من جهاز حقيقي؛ نقاط بلا BranchId مستبعدة (موثّق) |
| غير محسوم | لا |

## القواعد المثبتة
- يوم تقويمي بغداد 00:00→+1 — **ليس** قطع بيع المبيعات 03:00.
- ActorKind=SalesEmployee فقط؛ لا خلط مع المتابعين.
- التفويض التاريخي: `BranchId` على النقطة ∈ فروع القارئ — لا يشترط تكليفًا ساريًا اليوم.
- نقاط بلا BranchId: لا تُنسب بالتخمين.
- Markers فقط؛ فجوات >30د تُعلن؛ Truncated صريح مع صفحات.
- SalesLocation.Read وSalesEmployee.Read لا تمنحان المسار.

## ملفات
- `Application/SalesEmployeeTracking/**/GetRoute/*`
- `SalesEmployeeTrackingDataService.GetRouteAsync`
- `panel SalesRoutes/`
- روابط من موظفي المبيعات عند SalesRoute.Read

## نواقص سابقة محفوظة (لم تُغلق هنا)
- 10: Flutter delegate-notes write
- 11: Mandob login lock collection
- 10–17: live/device verification
- 16: SalesFollowup Excel export customers
