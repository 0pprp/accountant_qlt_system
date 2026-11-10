namespace Application.FollowerWork.Common;

public static class FollowerWorkErrors
{
    public static readonly Error AccessDenied = new(
        "ليس لديك صلاحية قراءة دليل المتابعين.",
        "FollowerWork.AccessDenied");

    public static readonly Error BranchAccessDenied = new(
        "الفرع المطلوب خارج نطاق صلاحياتك.",
        "FollowerWork.BranchAccessDenied");

    public static readonly Error InvalidSort = new(
        "ترتيب غير مسموح.",
        "FollowerWork.InvalidSort");

    public static readonly Error FollowerNotFound = new(
        "المتابع غير موجود أو خارج نطاق التكليف المسموح.",
        "FollowerWork.FollowerNotFound");

    public static readonly Error FollowerBranchMismatch = new(
        "المتابع غير مرتبط بالفرع المطلوب عبر تكليف قائمة.",
        "FollowerWork.FollowerBranchMismatch");

    public static readonly Error CollectionConcurrencyConflict = new(
        "تعارض إصدار حالة الجباية — أعد التحميل.",
        "FollowerCollection.ConcurrencyConflict");

    public static readonly Error CollectionManageDenied = new(
        "ليس لديك صلاحية تشغيل/إيقاف الجباية.",
        "FollowerCollection.ManageDenied");

    public static readonly Error CollectionListOutOfScope = new(
        "القائمة خارج نطاق الفروع أو التكليفات المسموحة.",
        "FollowerCollection.ListOutOfScope");

    public static readonly Error MandobCollectionLocked = new(
        "جباية المتابع مفعّلة على هذه القائمة — لا يمكن للمندوب تسجيل دفعة.",
        "FollowerCollection.MandobLocked");

    public static readonly Error DelegateNotesWriteDenied = new(
        "ليس لديك صلاحية كتابة ملاحظات المندوبين.",
        "FollowerDelegateNotes.WriteDenied");

    public static readonly Error DelegateNotesListOutOfScope = new(
        "لا يمكنك إضافة ملاحظة على مندوب قائمة غير مسندة.",
        "FollowerDelegateNotes.ListOutOfScope");
}
