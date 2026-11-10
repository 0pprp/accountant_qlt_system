using Domain.Entities.SalesRequestAggregate.Enums;

namespace Domain.Services.SalesRequestLifecycle;

/// <summary>
/// تبويبات عرض الموظف/اللوحة — منفصلة عن Order.WorkflowStatus.
/// العروض الإضافية (sent/exceptions/deleted منفَّذة؛ legal غير منفَّذ) خارج سير العمل — 04E.
/// </summary>
public static class SalesRequestTabMap
{
    public const string Unassigned = "unassigned";
    public const string Incoming = "incoming";
    public const string Prepared = "prepared";
    public const string Inspected = "inspected";
    public const string Pending = "pending";
    public const string Rejected = "rejected";
    public const string Sold = "sold";
    public const string All = "all";

    public const string Sent = "sent";
    public const string Exceptions = "exceptions";
    public const string Legal = "legal";
    public const string Deleted = "deleted";

    /// <summary>عروض إضافية متداخلة مع تبويبات سير العمل — لا تدخل في «الكل» ولا في مجموع العدادات.</summary>
    public static readonly string[] ExtraViewsImplemented = [Sent, Exceptions, Deleted, Legal];

    /// <summary>لا عروض إضافية معلّقة بعد تفعيل القانونية (دفعة 21).</summary>
    public static readonly string[] ExtraViewsNotImplemented = [];

    /// <summary>
    /// مصادر «المرسلة»: تقديم من شخص (موظف/متابع/مندوب). Manual وExcelImport وExternalCrm مستبعدة.
    /// قيم enum فقط — لا مسار إدخال مربوط لأي مصدر حتى الآن (انظر Sources-04E.md).
    /// </summary>
    public static readonly SalesRequestSourceType[] SentSourceTypes =
    [
        SalesRequestSourceType.EmployeeSubmitted,
        SalesRequestSourceType.Follower,
        SalesRequestSourceType.Delegate
    ];

    /// <summary>مصادر لها مسار إدخال مربوط فعليًا في الشيفرة: لا شيء.</summary>
    public static readonly SalesRequestSourceType[] IngestionWiredSourceTypes = [];

    public static readonly string[] WorkflowTabs =
    [
        Unassigned, Incoming, Prepared, Inspected, Pending, Rejected, Sold
    ];

    public static IReadOnlyList<SalesRequestStatus> StatusesForTab(string tab) =>
        Normalize(tab) switch
        {
            Incoming =>
            [
                SalesRequestStatus.Assigned,
                SalesRequestStatus.Viewed,
                SalesRequestStatus.Returned
            ],
            Prepared =>
            [
                SalesRequestStatus.PreparedForSale,
                SalesRequestStatus.InProgress,
                SalesRequestStatus.ConvertedToSale
            ],
            Inspected => [SalesRequestStatus.Inspected],
            Pending => [SalesRequestStatus.Pending],
            Rejected => [SalesRequestStatus.Rejected],
            Sold => [SalesRequestStatus.Completed],
            _ => []
        };

    public static bool IsUnassignedTab(string? tab) => Normalize(tab) == Unassigned;

    public static bool IsAllTab(string? tab) => Normalize(tab) == All;

    public static bool IsExtraNotImplemented(string? tab) =>
        ExtraViewsNotImplemented.Contains(Normalize(tab));

    public static bool IsExtraImplemented(string? tab) =>
        ExtraViewsImplemented.Contains(Normalize(tab));

    public static string Normalize(string? tab) =>
        (tab ?? Incoming).Trim().ToLowerInvariant().Replace('-', '_');

    public static string DisplayTabForStatus(SalesRequestStatus status) => status switch
    {
        SalesRequestStatus.New => Unassigned,
        SalesRequestStatus.Assigned or SalesRequestStatus.Viewed or SalesRequestStatus.Returned => Incoming,
        SalesRequestStatus.PreparedForSale or SalesRequestStatus.InProgress
            or SalesRequestStatus.ConvertedToSale => Prepared,
        SalesRequestStatus.Inspected => Inspected,
        SalesRequestStatus.Pending => Pending,
        SalesRequestStatus.Rejected => Rejected,
        SalesRequestStatus.Completed => Sold,
        SalesRequestStatus.LegalFollowup => "legal",
        _ => Incoming
    };

    public static string ArabicLabel(string tab) => Normalize(tab) switch
    {
        Unassigned => "غير المسند",
        Incoming => "طلبات البيع",
        Prepared => "جاهز للبيع",
        Inspected => "تم الكشف",
        Pending => "معلقة",
        Rejected => "مرفوض",
        Sold => "تم البيع",
        All => "الكل",
        "sent" => "الطلبات المرسلة",
        "exceptions" => "الاستثناءات",
        "legal" => "القانونية",
        "deleted" => "المحذوفة",
        _ => tab
    };
}
