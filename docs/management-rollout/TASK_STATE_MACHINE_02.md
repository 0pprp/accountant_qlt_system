# دورة المهام الإدارية — مستخرجة من المرجع القديم

مصدر: `SalesHaider-main` — `DirectorAssignedTaskRules` / `DirectorAssignedTaskService` + تطبيقات BM/DM.
لا اختراع من أسماء الأزرار. التعارض (`expectedVersion`) جديد في النظام الحالي (المرجع كان last-write-wins).

## الحالات

| قيمة مخزّنة (byte) | المعنى |
|---:|---|
| 0 Sent | مرسلة — بانتظار قبول/رفض الحامل |
| 1 Transferred | منقولة — بعد نقل المفوض لمدير فرع |
| 2 Accepted | مقبولة — قيد الإنجاز لدى الحامل |
| 3 Returned | معادة — مرفوضة بملاحظة؛ يعيد المنشئ إرسالها |
| 4 AwaitingDelegatedConfirm | بانتظار تصديق المفوض — بعد إنجاز الفرع لمهمة منقولة |
| 5 Completed | منجزة — نهائية |

## جدول الانتقالات

| الحالة الحالية | الإجراء | المنفّذ المسموح | الحالة التالية | إلزامي |
|---|---|---|---|---|
| — | Create | Task.Create (+ نطاق فرع) | Sent | عنوان، أولوية، استحقاق، مكلَّف من DB |
| Sent أو Transferred | Accept | حامل المهمة + Task.Accept | Accepted | expectedVersion |
| Sent أو Transferred | Reject | حامل المهمة + Task.Reject | Returned | سبب رفض + expectedVersion |
| Returned | Assign/Resend | Task.Assign + Task.Create نطاق | Sent | إعادة تعيين اختيارية؛ مسح RejectNote وTransferredAt |
| Accepted | Transfer | مفوض = الحامل الأصلي Assignee + Task.Transfer | Transferred | targetUserId من DB (مدير فرع في نفس الفرع)؛ لا displayName موثوق من العميل |
| Accepted | Complete (Assignee=فرع أو لجنة، بلا مسار تصديق) | الحامل + Task.Complete | Completed | expectedVersion |
| Accepted | Complete (مفوض بلا نقل نشط) | الحامل المفوض + Task.Complete | Completed | self-complete — لا Confirm |
| Accepted | Complete (بعد نقل؛ الحامل مدير فرع) | الحامل + Task.Complete | AwaitingDelegatedConfirm | يعاد الحامل إلى Assignee الأصلي |
| AwaitingDelegatedConfirm | Confirm | Assignee الأصلي (مفوض) + Task.Confirm | Completed | لا تأكيد ذاتي لمسار بلا نقل؛ expectedVersion |

## ما لا يمر بالتأكيد

- مهمة مكلَّفة مباشرة لمدير فرع أو مسؤول لجنة: Accept → Complete → Completed
- مفوض ينجز بنفسه دون نقل: Accept → Complete → Completed
- Accept / Reject / Transfer / Create / Resend / رفع مرفق — بلا تصديق ثانٍ

## مرفقات

- حجم ≤ 20MB؛ امتدادات: jpg/jpeg/png/webp/gif/pdf/doc/docx/xls/xlsx
- التنزيل يراجع صلاحية المهمة + Task.ReadAttachment ونطاق الفرع/الظهور

## عزل الفرع

- `BranchId` على المهمة؛ كل قراءة/كتابة/تنزيل تتحقق من نطاق حساب الممثل.
- بعد النقل A→B: حامل A يفقد إجراءات الحامل؛ Confirm فقط لـ Assignee الأصلي.
