# سجل شاشات 09 — لوحة المبيعات + محتوى + صلاحيات

| الشاشة | مرجع قديم | مدخل جديد | كاتب/قارئ | حالات | API | صلاحية | نطاق | مقارنة |
|---|---|---|---|---|---|---|---|---|
| نظرة عامة | sales-manager-dashboard | `/sale/overview` | SalesOverview | Available/Unavailable metrics | GET admin/SalesOverview | SalesOverview.Read | BranchIds | مطابقة قطع 03:00 بغداد |
| طلبات البيع | sales-manager-requests | `/sale/requests` (تحت sale) | SalesRequests + Hub | tabs+realtime | admin SalesRequests + hubs/sales-requests | Tab* / Read* | session branches؛ مجموعات Hub من هوية السيرفر | مطابقة |
| موظفين | sales-manager-employees | `/sale/employees` | SalesEmployeeAssignment | grant/end | admin SalesEmployees | SalesEmployee.* | BranchIds | تكليف لا Mandob وحده |
| متابعة | sales-manager-followers (مبيعات) | `/sale/followup` | SalesFollowup | cards | admin SalesFollowup | SalesFollowup.Read | BranchIds | موظفو مبيعات |
| خريطة | sales-manager-map | `/sale/live-map` | SalesLocation Actor=SalesEmployee | live | admin SalesLocations | SalesLocation.Read | BranchIds | مسار يوم تقويمي منفصل |
| مسارات | sales-manager-routes | `/sale/routes` | SalesRoute | day Baghdad 00:00 | admin SalesRoutes | SalesRoute.Read | BranchIds | ليس قطع 03:00 |
| مبيعات منجزة | sales-manager-sales | `/sale/completed` | Orders | SaleDate | admin Orders / SalesRecords | Order.Read | BranchIds | مطابقة |
| ملخص | sales-manager-sales-summary | `/sale/summary` | SalesSummary | matrix | admin SalesSummary | SalesSummary.Read | BranchIds | يوم تقويمي موثّق ≠ 03:00 |
| قانونية | sales-manager-legal-followups | `/sale/legal` | LegalFollowup | cases | admin LegalFollowups | LegalFollowup.* | BranchIds | مطابقة |
| Excel بحث | sales-manager-excel-search | `/sale/excel-search` **جديد 09** | Customer names | found/missing | POST admin/SalesExcelSearch/customers | Customer.Read | BranchIds | مُغلق الآن |
| مشتريات | sales-manager-purchases | `/purchases` (finance) | Purchase module | list/create | admin Purchases | Purchase.Read | موجود مسبقًا | مدخل مالي لا تحت sale — مقصود |
| PriceBoard | director-price-board | management oversight/price-board | PriceBoard ads | governorate | admin PriceBoard | PriceBoard.* | محافظة | ليس Product CRUD |
| إعلانات | director-announcements | oversight/announcements | Announcement | AppKey+image+enabled | admin Announcements | Announcement.* | AppKey؛ Follower=Motaba | مطابقة |
| Tutorial | director-tutorial-videos | oversight/tutorials | Tutorial Q+URL | https allowlist YouTube/Vimeo **تشديد 09** | admin TutorialVideos | Tutorial.* | AppKey | تشديد روابط |
| مهام لجنة | director-committee-tasks | oversight/committee-tasks | CommitteeTask | ≠ ManagementTask | admin CommitteeTasks | CommitteeTask.* | — | منفصل |
| AppNote | director-notes | oversight/app-notes | AppNote | ≠ Complaint | admin AppNotes | AppNote.* | — | منفصل |
| صلاحيات | User powers | PowersForm | Inherit/Allow/Deny | baseline+expectedVersion | Patch permission overrides | User.ManagePermissions | لا Admin bypass بالاسم | ثلاثي الحالات |

## يوم العمل

- قطع البيع 03:00 بغداد: `SalesWorkDayIraq` / نظرة عامة المبيعات.
- مسار الموقع: يوم تقويمي بغداد 00:00→+1 — موثّق في SalesEmployeeTracking؛ لا دمج تلقائي.
