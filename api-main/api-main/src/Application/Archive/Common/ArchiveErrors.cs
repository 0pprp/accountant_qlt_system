namespace Application.Archive.Common;

public static class ArchiveErrors
{
    public static Error BundleNotFound = new("لم يتم العثور على الحزمة", "Archive_Bundle_Not_Found");
    public static Error TransactionNotFound = new("لم يتم العثور على المعاملة", "Archive_Transaction_Not_Found");
    public static Error ActionForbidden = new("لا تملك صلاحية هذا الإجراء", "Archive_Action_Forbidden");
    public static Error BranchForbidden = new("هذا الفرع خارج نطاقك", "Archive_Branch_Forbidden");
    public static Error StageForbidden = new("هذه المرحلة ليست مرحلتك", "Archive_Stage_Forbidden");
    public static Error NotBundleOwner = new("الحزمة ليست لك", "Archive_Not_Bundle_Owner");

    public static Error PeriodNotClosed =
        new("لا يمكن إرسال الحزمة قبل انتهاء عشرتها", "Archive_Period_Not_Closed");

    public static Error NothingToSend = new("لا توجد معاملات جاهزة للإرسال", "Archive_Nothing_To_Send");
    public static Error NothingToReceive = new("لم تُحدَّد معاملات للاستلام", "Archive_Nothing_To_Receive");
    public static Error ReturnReasonRequired = new("سبب الاسترجاع إجباري", "Archive_Return_Reason_Required");
    public static Error InvalidReturnDestination = new("وجهة الاسترجاع غير صالحة", "Archive_Invalid_Return_Destination");
    public static Error ConcurrencyConflict = new("تغيّرت المعاملة أثناء العمل — أعد التحميل", "Archive_Concurrency_Conflict");
}
