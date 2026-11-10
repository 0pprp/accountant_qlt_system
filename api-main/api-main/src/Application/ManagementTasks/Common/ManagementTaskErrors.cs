using Application.Common;

namespace Application.ManagementTasks.Common;

public static class ManagementTaskErrors
{
    public static Error NotFound = new("المهمة غير موجودة.", "Task_NotFound");
    public static Error ActionForbidden = new("غير مصرح بتنفيذ هذا الإجراء", "Task_Action_Forbidden");
    public static Error BranchForbidden = new("الفرع خارج نطاق حسابك", "Task_Branch_Forbidden");
    public static Error BranchRequired = new("الفرع مطلوب", "Task_Branch_Required");
    public static Error ConcurrencyConflict = new(
        "تم تغيير حالة المهمة بواسطة عملية أخرى — أعد التحميل",
        "Task_Concurrency_Conflict");
    public static Error InvalidTransition = new("لا يمكن تنفيذ الإجراء في الحالة الحالية", "Task_Invalid_Transition");
    public static Error RejectNoteRequired = new("ملاحظة الرفض مطلوبة.", "Task_RejectNote_Required");
    public static Error AssigneeRequired = new("المكلَّف مطلوب", "Task_Assignee_Required");
    public static Error AssigneeNotEligible = new("المكلَّف غير مؤهل أو خارج نطاق الفرع", "Task_Assignee_NotEligible");
    public static Error TransferTargetNotEligible = new("مستخدم الفرع غير موجود أو غير مؤهل", "Task_TransferTarget_NotEligible");
    public static Error AttachmentForbidden = new("غير مصرح بتنزيل هذا المرفق", "Task_Attachment_Forbidden");
    public static Error AttachmentNotFound = new("المرفق غير موجود", "Task_Attachment_NotFound");
    public static Error AttachmentTooLarge = new("حجم الملف يتجاوز 20 ميغابايت", "Task_Attachment_TooLarge");
    public static Error AttachmentTypeDenied = new(
        "صيغة الملف غير مدعومة (صور، PDF، Word، Excel).",
        "Task_Attachment_TypeDenied");
    public static Error AttachmentRequired = new("الملف مطلوب.", "Task_Attachment_Required");
    public static Error LinkedEntityBranchMismatch = new(
        "الطلب أو الزبون ليس ضمن فرع المهمة",
        "Task_LinkedEntity_BranchMismatch");
    public static Error TitleRequired = new("عنوان المهمة مطلوب", "Task_Title_Required");
    public static Error DueRequired = new("تاريخ الاستحقاق مطلوب.", "Task_Due_Required");
    public static Error InvalidPriority = new("أولوية غير صالحة.", "Task_Invalid_Priority");
}
