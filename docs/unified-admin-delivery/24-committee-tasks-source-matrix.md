# 24 — مصفوفة مصدر مهام اللجنة

## طبيعة الكيان (من المصدر)
**ليست** دورة قبول/إنجاز مثل ManagementTasks.
هي **نشرة مهام/تعليمات نصية** يديرها المدير (Director policy في المرجع):
- `Body` نص طويل
- `IsEnabled` تفعيل العرض
- `SortOrder` ترتيب
تظهر ككروت في تطبيق مسؤول اللجنة؛ الضغط يعرض النص كاملًا.
لا قبول ولا رفض ولا تصديق ولا تكليف شخص.

مرجع: `FE_Company/src/pages/director-committee-tasks.vue` + `AppCommitteeTasks` + `SalesManagerCommitteeTasksController` + `MobileCommitteeTasksController`.

## تمييز الأدوار
| الدور | علاقته |
|---|---|
| مدير/لوحة الإدارة | CRUD النشرة |
| مسؤول اللجنة (Motaba/Follower app) | قراءة المفعّل فقط |
| ManagementTasks / مفوض / مندوب بيع | كيان مختلف — لا خلط |

## قرار المعمارية
**موديول منفصل `CommitteeTasks`** — سبب الفصل: دورة العمل كتالوج تمكين وليست مهمة مسندة بحالات. لا إعادة استخدام ManagementTasks بعنوان مختلف.

## انتقالات/إجراءات المصدر
| الحالة | الإجراء | الفاعل | الشروط | النتيجة |
|---|---|---|---|---|
| — | إنشاء | مدير مخوّل | Body غير فارغ ≤8000 | صف جديد SortOrder=max+1 |
| موجود | تعديل | مدير | Body صالح | تحديث Body/IsEnabled |
| موجود | حذف | مدير | — | حذف صف |
| مفعّل | قراءة عامة للتطبيق | تطبيق اللجنة | IsEnabled وBody غير فارغ | قائمة مرتبة |

## المقابل الحالي في الجديد
| عنصر | الحالة |
|---|---|
| تبويب `oversight/committee-tasks` | `ready` — CommitteeTasksPage |
| API/Domain | `CommitteeTaskItems` + CQRS Admin/App |
| تطبيق اللجنة `TasksPage` | مربوط بـ `GET /api/v1/app/committee/tasks` |

## فجوات متبقية بعد التنفيذ
1. API-live على iso
2. Browser-live لقطات
3. Device-verified لتطبيق اللجنة
