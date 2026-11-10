namespace Application.AppNotes.Common;

public static class AppNoteErrors
{
    public static readonly Error Forbidden = new("غير مخول.", "AppNote.Forbidden");
    public static readonly Error NotFound = new("الملاحظة غير موجودة.", "AppNote.NotFound");
    public static readonly Error BodyShort = new("نص الملاحظة قصير جدًا (الحد الأدنى 10 أحرف).", "AppNote.BodyShort");
    public static readonly Error BodyLong = new("الحد الأقصى للملاحظة 2000 حرف.", "AppNote.BodyLong");
    public static readonly Error NoActor = new("المستخدم غير معروف.", "AppNote.NoActor");
}
