# 18 — مصفوفة مصدر مسارات موظفي المبيعات

مصدر: `sales-manager-routes.vue` + composables + `salesManagerApi` route endpoints.
جديد: `GET /api/v1/admin/SalesEmployeeTracking/{employeeId}/route` + `/sale/routes`.

| العنصر | دليل المصدر | المعنى | المقابل الجديد | الصلاحية | اختبار القبول |
|---|---|---|---|---|---|
| اختيار الموظف | dropdown موظفين | هوية موظف مبيعات | `employeeId` + نقاط `ActorKind=SalesEmployee` | SalesRoute.Read | Location/Employee.Read وحدها تُرفض |
| الفترة | date واحد | يوم عرض | `DateOnly date` | SalesRoute.Read | URL validated `date` |
| حدود اليوم | مرجع قديم قد يستخدم دوام/03:00 | نافذة النقاط | **تقويمي بغداد 00:00→+1** عبر `IraqCalendar.DayBoundsUtc` | موثّق | ≠ `SalesWorkDayIraq` 03:00 |
| ترتيب النقاط | CapturedAt | زمني ثابت | `RecordedAtUtc` ثم `Id` | — | وصول متأخر لا يعيد الترتيب |
| التوقفات/الفجوات | فجوات زمنية | إعلان فقط | `Gaps[]` عند >30 دقيقة | — | بلا polyline يوحي بمسار متصل |
| مسافة/سرعة/زيارة زبون | غير مثبتة كوظيفة | — | غير معروضة | — | لا مسافة طريق؛ لا استنتاج زيارة |
| فلاتر دوام/رحلة | غير مثبتة في المرجع | — | غير منقولة | — | — |
| تفويض تاريخي | فرع وقت الالتقاط | لا نقل لفرع جديد | `point.BranchId ∈` نطاق القارئ | SalesRoute.Read | انتقال A→B لا ينسب نقاط A لـ B |
| تكليف منتهٍ | قراءة تاريخ | أهلية التنفيذ ≠ القراءة | لا يشترط assignment ساري | SalesRoute.Read | نقاط ضمن فروع مخوّلة تُقرأ |
| نقاط بلا BranchId | بيانات ناقصة | لا تخمين | مستبعدة من الاستعلام | — | لا تظهر منسوبة |
| Truncation | صفحات | استكمال صريح | `Truncated` + page/totalPages | — | بلا truncation صامت |
| إرسال مواقع | خارج النطاق | — | لا endpoint جديد | — | يعيد استخدام ingest 17 |

روابط من 15/17 بعد الاكتمال: مسار الموظف يظهر عند `SalesRoute.Read` فقط.
