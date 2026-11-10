# Coverage 23 — المهام الإدارية

## الحالة
| المحور | الحكم |
|---|---|
| Implemented | وحدة ManagementTasks + مهامي مفوض/فرع عبر `inboxOnly`؛ انتقالات Domain؛ InboxRules؛ مرفقات محروسة؛ Outbox/Hub + عميل لوحة SignalR مع polling معلن |
| Unit-tested | ManagementTaskDomain*؛ ManagementTaskInboxRulesTests |
| API-live | نعم — self-complete task#2→Completed؛ transfer→accept→complete→AwaitingConfirm→confirm task#3 (evidence/23-management-tasks/live-api.json) |
| Browser-live | صفحة المهام موجودة؛ فصل مهامي عن الإدارة عبر router — لقطات هذه الجلسة Missing |
| Device-verified | غير مطلوب لتطبيقات الإدارة الويب |
| Missing | Browser-live لقطات؛ تعارض concurrency صريح كسيناريو مستقل |

## قواعد مثبتة
- التصديق بعد نقل المفوض وإنجاز الفرع فقط.
- الإنجاز الذاتي (حامل=مكلَّف بلا نقل لآخر) → Completed مباشرة.
- inboxForMe يشمل Returned وAwaitingDelegatedConfirm للمكلَّف الأصلي.

## الشاشات
| شاشة | مسار | inboxOnly |
|---|---|---|
| إدارة المهام | canonical ManagementTasks | false |
| مهامي المفوض | assignedTasks | true |
| مهامي الفرع | branchAssignedTasks | true |

## نواقص سابقة
لا إسقاط لنواقص 10–21 المفتوحة بلا دليل إغلاق.
