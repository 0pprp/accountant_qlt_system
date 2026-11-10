# 14 — مصفوفة مصدر نظرة عامة للمبيعات

مصدر: `sales-manager-dashboard.vue` ← `GET .../dashboard` ← `SalesManagerQueryService.DashboardAsync`.

| العرض | مصدر الكود | المعنى | الفترة/النطاق | المقابل الجديد | الصلاحية |
|---|---|---|---|---|---|
| موظفين في الدوام | EmployeesOnShift | ShiftStatus=Active | الآن / فرع | SalesShift نشط لموظفي المبيعات (دور Mandob) | SalesOverview.Read |
| بدون دوام | EmployeesOffShift | ليس Active | الآن | بدون SalesShift نشط | SalesOverview.Read |
| الموقع المباشر | LiveLocations | Active + LocationStatus=Live | الآن ≤150ث | نقطة SalesLocationPoints حديثة + دوام | SalesOverview.Read (عدد فقط؛ لا إحداثيات) |
| المبيعات اليوم | SalesToday | مسودات Completed في نافذة يوم العمل | cutoff 03:00 بغداد | Orders Approved بـ SaleDate = يوم العمل بغداد | SalesOverview.Read |
| الطلبات المعلقة | PendingSales | SalesRequest Status=Pending | الكل غير المحذوف | SalesRequestStatus.Pending | SalesOverview.Read |
| طلبات جديدة | NewSalesRequests | Status=New | الكل غير المحذوف | SalesRequestStatus.New | SalesOverview.Read |

**لا:** SalesRequest.Completed كبديل للمبيعات؛ login كحضور؛ إحداثيات GPS في نظرة عامة.

## اختبار القبول (مختصر)
| سيناريو | متوقع |
|---|---|
| صفر موظفين | قيم 0 Available وليست Unavailable |
| فشل API | UI خطأ — ليس «لا نشاط» |
| Overview.Read بلا Employee.Read | أرقام ظاهرة؛ روابط الدوام معطّلة |
| Overview.Read بلا TabPending | عداد Pending ظاهر؛ الرابط معطّل |
| فرع خارج النطاق | BranchForbidden |
| SaleDate يوم العمل قبل 03:00 | يُحسب على اليوم السابق ببغداد |

## تمكين مستقل
`saleOverview` ready عند اكتمال الربط — منفصل عن `saleEmployees`.
