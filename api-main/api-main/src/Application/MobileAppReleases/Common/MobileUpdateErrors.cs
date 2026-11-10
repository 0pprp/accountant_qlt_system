namespace Application.MobileAppReleases.Common;

public static class MobileUpdateErrors
{
    public static readonly Error Forbidden = new("غير مخول.", "MobileUpdate.Forbidden");
    public static readonly Error UnknownApp = new("التطبيق غير معروف.", "MobileUpdate.UnknownApp");
    public static readonly Error AppKey = new("مفتاح التطبيق غير مدعوم.", "MobileUpdate.AppKey");
    public static readonly Error ApkUrlRequired = new("رابط التحميل مطلوب عند التحديث الإجباري.", "MobileUpdate.ApkUrl");
    public static readonly Error ApkUrlInvalid = new("رابط التحميل يجب أن يبدأ بـ http.", "MobileUpdate.ApkUrlInvalid");
    public static readonly Error Concurrency = new("تعارض نسخة السجل.", "MobileUpdate.Concurrency");
    public static readonly Error Validation = new("بيانات الإصدار غير صالحة.", "MobileUpdate.Validation");
}
