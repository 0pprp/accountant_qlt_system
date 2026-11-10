# Coverage 24 — مهام اللجنة

## الحكم
| المحور | الحالة |
|---|---|
| Implemented | نعم — موديول `CommitteeTasks` منفصل عن ManagementTasks (نشرة Body/IsEnabled/SortOrder) |
| Unit-tested | نعم — Domain + handlers (إنشاء/تعارض نسخة/قائمة التطبيق المفعّلة فقط/رفض بلا AppRead) |
| API-live | Missing في هذه الجلسة (هجرة جاهزة؛ لم تُطبَّق على iso حي هنا) |
| Browser-live | صفحة اللوحة جاهزة تحت `oversight/committee-tasks` — لقطات Missing |
| Device-verified | TasksPage مربوط بـ `/api/v1/app/committee/tasks` — إثبات جهاز Missing |
| Missing | API-live / Browser shots / Device |

## طبيعة مثبتة من المصدر
ليست دورة قبول/إنجاز. هي نشرة تعليمات يظهر نصها في تطبيق مسؤول اللجنة عند `IsEnabled`.

## الشاشات
| شاشة | مسار | صلاحية الدخول |
|---|---|---|
| لوحة الإدارة ← مهام اللجنة | `/management/oversight/committee-tasks` | `CommitteeTask.Read` |
| تطبيق اللجنة ← مهام | `TasksPage` | `CommitteeTask.AppRead` |

## Endpoints
- `GET/POST/PUT/DELETE /api/v1/admin/CommitteeTasks`
- `GET /api/v1/app/committee/tasks`

## نواقص سابقة تبقى مفتوحة
- 22 Device-verified لأزرار الإنشاء
- 23 Browser-live + concurrency مستقل

لا دفعة 26. لا نشر إنتاج.
