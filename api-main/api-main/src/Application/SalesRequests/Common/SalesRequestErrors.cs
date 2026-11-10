namespace Application.SalesRequests.Common;

public static class SalesRequestErrors
{
    public static Error NotFound = new("طلب البيع غير موجود", "SalesRequest_Not_Found");
    public static Error NotUnassigned = new("الطلب ليس في حالة غير مسند", "SalesRequest_Not_Unassigned");
    public static Error ExceptionAlreadyHeld = new("يوجد طلب استثناء قائم لهذا الطلب", "SalesRequest_Exception_Already_Held");
    public static Error InvalidEmployee = new("الموظف غير صالح للإسناد", "SalesRequest_Invalid_Employee");
    public static Error BranchForbidden = new("غير مصرح بالوصول إلى فرع هذا الطلب", "SalesRequest_Branch_Forbidden");
    public static Error InvalidCriterion = new("معيار المطابقة غير معروف", "SalesRequest_Invalid_Criterion");
    public static Error SchemaRequired = new(
        "جدول طلبات البيع غير مُطبَّق على قاعدة البيانات بعد",
        "SalesRequest_Schema_Required");
    public static Error BatchLimitExceeded = new(
        $"تجاوز حد الدفعة المعلن ({SalesRequestMatchBatchLimits.MaxRequestIds}) — قلّص عدد المعرّفات",
        "SalesRequest_Batch_Limit_Exceeded");
    public static Error ConcurrencyConflict = new(
        "تم تغيير حالة الطلب بواسطة عملية أخرى — أعد التحميل",
        "SalesRequest_Concurrency_Conflict");
    public static Error ExceptionNotAllowed = new(
        "لا يمكن طلب استثناء لهذه الحالة",
        "SalesRequest_Exception_Not_Allowed");
    public static Error ProfileForbidden = new(
        "غير مصرح بعرض بروفايل التدقيق",
        "SalesRequest_Audit_Profile_Forbidden");
    public static Error DocumentsForbidden = new(
        "غير مصرح بقراءة مستندات التدقيق",
        "SalesRequest_Audit_Documents_Forbidden");
    public static Error NotOwner = new(
        "الطلب غير مملوك للموظف الحالي",
        "SalesRequest_Not_Owner");
    public static Error IllegalTransition = new(
        "انتقال حالة غير مسموح",
        "SalesRequest_Illegal_Transition");
    public static Error NoteRequired = new(
        "السبب/الملاحظة مطلوبة",
        "SalesRequest_Note_Required");
    public static Error AlreadyLinked = new(
        "الطلب مرتبط ببيع آخر",
        "SalesRequest_Already_Linked");
    public static Error InvalidTab = new(
        "تبويب غير معروف",
        "SalesRequest_Invalid_Tab");
    public static Error TabForbidden = new(
        "غير مصرح بقراءة هذا التبويب",
        "SalesRequest_Tab_Forbidden");
    public static Error TabNotImplemented = new(
        "هذا العرض غير منفَّذ بعد",
        "SalesRequest_Tab_Not_Implemented");
    public static Error ActionForbidden = new(
        "غير مصرح بتنفيذ هذا الإجراء",
        "SalesRequest_Action_Forbidden");
    public static Error ExceptionNotPending = new(
        "لا يوجد طلب استثناء معلّق لهذا الطلب",
        "SalesRequest_Exception_Not_Pending");
    public static Error ExceptionSelfApproval = new(
        "لا يجوز لطالب الاستثناء الموافقة عليه بنفسه",
        "SalesRequest_Exception_Self_Approval");
    public static Error ExceptionRequesterUnknown = new(
        "تعذّر تحديد طالب الاستثناء من سجل الحركة — لا موافقة بدون هوية الطالب",
        "SalesRequest_Exception_Requester_Unknown");
    public static Error ExceptionPendingBlocksAssign = new(
        "يوجد طلب استثناء معلّق — لا إسناد قبل قرار الموافقة",
        "SalesRequest_Exception_Pending_Blocks_Assign");
    public static Error ExceptionRejectedBlocksAssign = new(
        "رُفض الاستثناء — اطلب استثناءً جديدًا أو ارفض الطلب",
        "SalesRequest_Exception_Rejected_Blocks_Assign");
    public static Error AlreadyDeleted = new(
        "الطلب محذوف مسبقًا",
        "SalesRequest_Already_Deleted");
    public static Error NotDeleted = new(
        "الطلب غير محذوف",
        "SalesRequest_Not_Deleted");
    public static Error DeleteCompletedForbidden = new(
        "لا يُحذف طلب مكتمل (تم البيع)",
        "SalesRequest_Delete_Completed_Forbidden");
    public static Error DeleteLinkedOrderForbidden = new(
        "لا يُحذف طلب مرتبط ببيع/مسودة — لا تأثير مالي من الحذف الناعم",
        "SalesRequest_Delete_Linked_Order_Forbidden");
    public static Error RestoreLinkedAssigneeIneligible = new(
        "لا تُستعاد صفقة مرتبطة ببيع وموظفها لم يعد مؤهلًا — راجع الإدارة",
        "SalesRequest_Restore_Linked_Assignee_Ineligible");
    public static Error CustomerRequiredForDraft = new(
        "لا يمكن إنشاء مسودة بيع دون زبون مرتبط بالطلب — أكمل المطابقة أولًا",
        "SalesRequest_Customer_Required_For_Draft");
    public static Error TransferSelfForbidden = new(
        "لا يُنقل الطلب إلى نفس الموظف",
        "SalesRequest_Transfer_Self_Forbidden");
    public static Error TransferPeerInvalid = new(
        "الزميل المستلم غير مؤهل أو خارج نطاق فرع الطلب",
        "SalesRequest_Transfer_Peer_Invalid");
    public static Error TransferTerminalForbidden = new(
        "لا يُنقل طلب مكتمل أو مرفوض أو محوّل للقانونية",
        "SalesRequest_Transfer_Terminal_Forbidden");
}
