# قبول المرحلة 02 — المهام وتبادل الإسناد

بيئة معزولة: `qalaat_local` · API `:4200` · Vite `:5173` · لا نشر إنتاج.

## Source-reviewed
- جدول الحالات من `DirectorAssignedTaskRules/Service` — انظر `TASK_STATE_MACHINE_02.md`
- التأكيد فقط بعد نقل مفوض→فرع ثم إنجاز الفرع؛ الإنجاز الذاتي بلا تصديق

## Implemented
- Domain Aggregate + Events + Attachments + ChangeOutbox
- Application CQRS (Create/Resend/Accept/Reject/Transfer/Complete/Confirm/Attachment)
- صلاحيات `Task.*` خارج `Permissions.All`؛ Admin صراحة فقط
- Controllers Admin + App؛ Hub realtime IDs/revision
- لوحة ManagementTasks على `/management/branch|delegate/assigned-tasks`
- Migration رسمية `20261108175747_ManagementTasks` مطبّقة على قاعدة معزولة

## Unit-tested
- `ManagementTaskDomainTests` — 9 اختبارات (قبول/رفض/نقل/تصديق/نسخة)

## Live-verified
انظر `evidence/02/results.json` (+ `ui_results.json` إن وُجد):
- دورة كاملة admin→transfer→gg→complete→confirm
- حساب بلا صلاحية 403
- تعارض confirm متزامن 200+409
- إنجاز ذاتي بلا تصديق + إعادة إرسال 409
- عزل قائمة الفرع (لا تسرّب branchId)

## Blocked
- نشر إنتاج
- نقرة UI كاملة لمبدّل الفرع إن فشل الترميز — يُعوَّض بـ Pinia + تأخير استجابة (عند تشغيل UI)
