# 23 — مصفوفة تكافؤ المهام الإدارية

## معنى الشاشات (آباء مستقلة)

| الشاشة | الموقع | المعنى | ربط التنفيذ |
|---|---|---|---|
| إدارة المهام (متابعة/إنشاء) | لوحة الإدارة — ManagementTasks canonical | لمن يملك `Task.Read` أو صلاحيات إدارة | `ManagementTasksPage` بدون `inboxOnly` |
| مهامي — المدير المفوض | قسم المفوض `assignedTasks` | صندوق المستخدم؛ ليست إدارة عامة | نفس الصفحة `inboxOnly: true` |
| مهامي — إدارة الفروع | قسم الفرع `branchAssignedTasks` | صندوق فرعي للمستخدم | نفس الصفحة `inboxOnly: true` |
| لا نسخة «مهامي» تحت الإشراف المكرر | — | ممنوع | لا embed إضافي تحت Sales |

## دورة الحالات (من المجال الحالي)

| إجراء | من → إلى | صلاحية | هوية/علاقة | ملاحظات |
|---|---|---|---|---|
| إنشاء/إسناد | → Sent | Create | منشئ | Concurrent stamp |
| قبول | Sent/Transferred → Accepted | Accept | حامل حالي | |
| رفض | → Returned | Reject | حامل + سبب | يظهر للمكلَّف الأصلي في مهامي |
| نقل | Accepted → Transferred | Transfer | مفوض→فرع | HasActiveTransfer |
| إنجاز ذاتي | Accepted → Completed | Complete | حامل=مكلَّف بدون نقل نشط لآخر | لا ينتظر تصديقًا |
| إنجاز بعد نقل | Accepted → AwaitingDelegatedConfirm | Complete | حامل الفرع | يعيد الحامل للمكلَّف |
| تصديق | AwaitingDelegatedConfirm → Completed | Confirm | المكلَّف الأصلي فقط + نقل سابق | |
| إعادة إرسال | Returned → Sent | Resend | المكلَّف الأصلي | |

## inboxForMe (مثبت)

ليس `CurrentHolderUserId` وحده:
1. الحامل الحالي يرى المهمة دائمًا.
2. المكلَّف الأصلي يرى `Returned` و`AwaitingDelegatedConfirm` حتى إن لم يكن الحامل.
3. الافتراضي لصندوق مفتوح يستبعد `Completed` فقط.

مرجع: `ManagementTaskInboxRules` + `GetManagementTasksHandler` + `ManagementTaskInboxRulesTests`.

## المرفقات والتنبيهات

| بند | الحكم |
|---|---|
| تنزيل | `Task.ReadAttachment` + ارتباط بالمهمة + CanSeeTask (فرع/دور) |
| لا مسار مكشوف | عبر endpoint بمصادقة؛ لا تخمين id كافٍ |
| نوع/حجم | سياسة المشروع في أمر الرفع |
| السجل | أحداث ManagementTask timeline |
| Realtime | Outbox + ManagementTaskHub + عميل لوحة `useManagementTaskRealtime` (متصل فقط عند Connected؛ polling عند الانقطاع) |
| فتح التفاصيل | لا يغيّر الحالة |

## التنفيذ مقابل المرجع

| مسار | الحالة |
|---|---|
| Domain transitions Confirm-after-transfer | Implemented |
| Self-complete بدون تصديق | Implemented |
| Inbox rules أوسع من holder | Implemented + Unit |
| فصل شاشات مهامي/إدارة | Implemented عبر `inboxOnly` + router props |
| مرفقات محروسة | Implemented |
| Device/Browser live لدورة مفوض↔فرع كاملة على iso | يُثبت في التسليم الحي |
