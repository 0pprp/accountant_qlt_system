using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Archive.Common;

/// <summary>
/// التسميات العربية في مكان واحد — تُعرض في اللوحة وفي التطبيقين،
/// فلا تُكتب ثلاث مرات ولا تتباعد.
/// </summary>
public static class ArchiveLabels
{
    public static string Status(ArchiveTransactionStatus status) => status switch
    {
        ArchiveTransactionStatus.WithSalesEmployee => "عند موظف المبيعات",
        ArchiveTransactionStatus.SentToBranch => "بانتظار استلام إدارة الفرع",
        ArchiveTransactionStatus.AtBranch => "بعهدة إدارة الفرع",
        ArchiveTransactionStatus.SentToFollower => "بانتظار استلام المتابع",
        ArchiveTransactionStatus.AtFollower => "بعهدة المتابع",
        ArchiveTransactionStatus.SentToArchive => "بانتظار استلام الأرشيف",
        _ => "مؤرشفة"
    };

    public static string Stage(ArchiveStage stage) => stage switch
    {
        ArchiveStage.SalesEmployee => "موظف المبيعات",
        ArchiveStage.BranchManagement => "إدارة الفرع",
        ArchiveStage.Follower => "المتابع",
        _ => "الأرشيف"
    };

    public static string Event(ArchiveEventType type) => type switch
    {
        ArchiveEventType.Created => "دخلت الحزمة",
        ArchiveEventType.Sent => "أُرسِلت",
        ArchiveEventType.Received => "استُلمت",
        ArchiveEventType.SendUndone => "أُلغي الإرسال",
        ArchiveEventType.Returned => "استُرجِعت",
        ArchiveEventType.Voided => "أُبطِلت",
        ArchiveEventType.Revived => "أُعيدت للمسار",
        _ => "نُقلت لحزمة أخرى"
    };

    public static string Period(int year, int month, byte index)
    {
        var order = index switch
        {
            1 => "الأولى",
            2 => "الثانية",
            _ => "الثالثة"
        };
        return $"العشرة {order} — {month:00}/{year}";
    }

    /// <summary>المرحلة التي تحمل الورق الآن؛ المنقولة تُنسب لمرحلة مُرسِلها حتى تُستلم.</summary>
    public static ArchiveStage HoldingStage(ArchiveTransactionStatus status) => status switch
    {
        ArchiveTransactionStatus.WithSalesEmployee => ArchiveStage.SalesEmployee,
        ArchiveTransactionStatus.SentToBranch => ArchiveStage.SalesEmployee,
        ArchiveTransactionStatus.AtBranch => ArchiveStage.BranchManagement,
        ArchiveTransactionStatus.SentToFollower => ArchiveStage.BranchManagement,
        ArchiveTransactionStatus.AtFollower => ArchiveStage.Follower,
        ArchiveTransactionStatus.SentToArchive => ArchiveStage.Follower,
        _ => ArchiveStage.Archive
    };

    public static string Where(ArchiveBundle bundle, int archived, int total)
    {
        if (total == 0)
            return "لا معاملات بعد";
        if (archived == total)
            return "مؤرشفة بالكامل";
        if (bundle.SentCount == 0)
            return "لم تُرسَل بعد";
        return $"مؤرشف {archived} من {total}";
    }
}
