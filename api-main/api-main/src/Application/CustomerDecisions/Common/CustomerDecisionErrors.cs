namespace Application.CustomerDecisions.Common;

public static class CustomerDecisionErrors
{
    public static readonly Error Forbidden = new("غير مخول.", "CustomerDecision.Forbidden");
    public static readonly Error NoteRequired = new("ملاحظة القرار إلزامية.", "CustomerDecision.NoteRequired");
    public static readonly Error InvalidType = new("نوع القرار غير معروف.", "CustomerDecision.InvalidType");
    public static readonly Error CustomerNotFound = new("الزبون غير موجود.", "CustomerDecision.CustomerNotFound");
    public static readonly Error BranchDenied = new("الزبون خارج نطاق فروعك.", "CustomerDecision.BranchDenied");
    public static readonly Error NotInQueue = new("الزبون غير مؤهل لطابور الأسماء القانونية.", "CustomerDecision.NotInQueue");
    public static readonly Error ConversionNotFound = new("طلب التحويل غير موجود.", "LegalConversion.NotFound");
    public static readonly Error ConversionConcurrency = new("تعارض نسخة طلب التحويل.", "LegalConversion.Concurrency");
    public static readonly Error RejectReasonRequired = new("سبب الرفض إلزامي.", "LegalConversion.RejectReasonRequired");
}
