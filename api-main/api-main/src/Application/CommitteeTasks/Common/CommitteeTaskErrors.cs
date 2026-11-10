namespace Application.CommitteeTasks.Common;

public static class CommitteeTaskErrors
{
    public static readonly Error Forbidden = new("غير مخول.", "CommitteeTask.Forbidden");
    public static readonly Error NotFound = new("المهمة غير موجودة.", "CommitteeTask.NotFound");
    public static readonly Error BodyRequired = new("نص المهمة مطلوب.", "CommitteeTask.BodyRequired");
    public static readonly Error BodyTooLong = new("النص طويل جداً (الحد 8000 حرف).", "CommitteeTask.BodyTooLong");
    public static readonly Error Concurrency = new("تم تعديل السجل من جهة أخرى. أعد التحميل.", "CommitteeTask.Concurrency");
}
