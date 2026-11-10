namespace Application.Waivers.Common;

public static class WaiverErrors
{
    public static readonly Error Forbidden = new("غير مخول.", "Waiver.Forbidden");
    public static readonly Error NotFound = new("طلب التنازل غير موجود.", "Waiver.NotFound");
    public static readonly Error NoteRequired = new("ملاحظة الطلب إلزامية.", "Waiver.NoteRequired");
    public static readonly Error NoteTooLong = new("الحد الأقصى للملاحظة 2000 حرف.", "Waiver.NoteTooLong");
    public static readonly Error AmountRequired = new("مبلغ التنازل يجب أن يكون أكبر من صفر.", "Waiver.AmountRequired");
    public static readonly Error Concurrency = new("تم تعديل الطلب من جهة أخرى. أعد التحميل.", "Waiver.Concurrency");
    public static readonly Error InvalidTransition = new("انتقال غير مسموح من الحالة الحالية.", "Waiver.InvalidTransition");
    public static readonly Error SelfDecide = new("لا يجوز لصاحب الطلب اعتماد أو رفض طلبه.", "Waiver.SelfDecide");
    public static readonly Error CustomerNotFound = new("الزبون غير موجود.", "Waiver.CustomerNotFound");
    public static readonly Error BranchDenied = new("الزبون خارج نطاق فروعك.", "Waiver.BranchDenied");
    public static readonly Error NothingToApply = new("لا يوجد متبقي قابل للتنازل.", "Waiver.NothingToApply");
}
