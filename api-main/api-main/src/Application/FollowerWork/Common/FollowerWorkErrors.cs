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

    public static readonly Error CustomerOutOfScope = new(
        "الزبون ليس ضمن قوائم هذا المتابع في الفروع المسموحة.",
        "FollowerWork.CustomerOutOfScope");

    public static readonly Error CollectionListNotFollowers = new(
        "القائمة ليست مسندة لهذا المتابع.",
        "FollowerCollection.ListNotFollowers");

    public static readonly Error ManageFollowersDenied = new(
        "ليس لديك صلاحية إدارة حسابات المتابعين.",
        "FollowerWork.ManageFollowersDenied");

    public static readonly Error FollowerHasLists = new(
        "لا يمكن حذف متابع مسندة إليه قوائم — انقل قوائمه لمتابع آخر أولاً.",
        "FollowerWork.FollowerHasLists");

    public static readonly Error FollowerAccountNotFound = new(
        "حساب المتابع غير موجود أو ليس بدور متابع.",
        "FollowerWork.FollowerAccountNotFound");

    public static readonly Error ListsOutOfScope = new(
        "بعض القوائم غير موجودة أو خارج الفروع المسموحة.",
        "FollowerWork.ListsOutOfScope");

    public static readonly Error UsernameTaken = new(
        "اسم المستخدم مستخدم مسبقاً.",
        "FollowerWork.UsernameTaken");

    public static readonly Error FollowerHasHistory = new(
        "لا يمكن حذف متابع له سجل عمل (مصادقات أو ملاحظات أو جباية) — الحساب محفوظ للتدقيق.",
        "FollowerWork.FollowerHasHistory");

    public static readonly Error DelegateNotesWriteDenied = new(
        "ليس لديك صلاحية كتابة ملاحظات المندوبين.",
        "FollowerDelegateNotes.WriteDenied");

    public static readonly Error DelegateNotesListOutOfScope = new(
        "لا يمكنك إضافة ملاحظة على مندوب قائمة غير مسندة.",
        "FollowerDelegateNotes.ListOutOfScope");
}
