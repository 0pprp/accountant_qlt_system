using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.ArchiveAggregate;

/// <summary>
/// حزمة عشرة واحدة لموظف مبيعات في فرع. الفرع جزء من المفتاح لأن الموظف قد يبيع
/// في فرعين داخل العشرة نفسها، ولو جمعناهما في حزمة واحدة لحملت فرعاً خاطئاً
/// لنصف معاملاتها فانكسر نطاق مسؤول الأرشيف بصمت.
///
/// الحزمة بلا حقل حالة: حالتها تُشتقّ من معاملاتها. العدّادات أدناه ذاكرة مؤقّتة
/// قابلة لإعادة البناء، وُجدت ليُمكن الفرز والتصفية عبر كل الحزم مع ترقيم صفحات.
/// </summary>
public class ArchiveBundle : AuditableEntity
{
    private ArchiveBundle()
    {
    }

    public ArchiveBundle(
        int branchId,
        int salesEmployeeUserId,
        string salesEmployeeName,
        ArchivePeriod period)
    {
        BranchId = branchId;
        SalesEmployeeUserId = salesEmployeeUserId;
        SalesEmployeeName = salesEmployeeName;
        PeriodYear = period.Year;
        PeriodMonth = period.Month;
        PeriodIndex = period.Index;
        PeriodStart = period.Start;
        PeriodEnd = period.End;
        Transactions = new HashSet<ArchiveTransaction>();
    }

    public int BranchId { get; private set; }
    public int SalesEmployeeUserId { get; private set; }
    public string SalesEmployeeName { get; private set; } = string.Empty;

    public int PeriodYear { get; private set; }
    public int PeriodMonth { get; private set; }
    public byte PeriodIndex { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }

    public int ItemCount { get; private set; }
    public int SentCount { get; private set; }
    public int ArchivedCount { get; private set; }
    public int OpenReturnCount { get; private set; }
    public DateTimeOffset? FirstSentAtUtc { get; private set; }

    public int ConcurrencyStamp { get; private set; }

    public Branch? Branch { get; private set; }
    public User? SalesEmployee { get; private set; }
    public ICollection<ArchiveTransaction> Transactions { get; private set; } = new HashSet<ArchiveTransaction>();

    public ArchivePeriod Period() => ArchivePeriod.For(PeriodYear, PeriodMonth, PeriodIndex);

    /// <summary>لا تُرسَل حزمة قبل أن تنتهي عشرتها.</summary>
    public bool CanSendOn(DateOnly baghdadToday) => Period().IsClosedOn(baghdadToday);

    /// <summary>
    /// الحزمة لا تكتمل أبداً: مبيع يُتمّ متأخراً بتاريخ داخل عشرة مُرسَلة يدخلها
    /// كعنصر جديد غير مُرسَل، فينمو المقام ويعود زر الإرسال صالحاً.
    /// </summary>
    public void ApplyCounters(int itemCount, int sentCount, int archivedCount, int openReturnCount)
    {
        ItemCount = itemCount;
        SentCount = sentCount;
        ArchivedCount = archivedCount;
        OpenReturnCount = openReturnCount;
        ConcurrencyStamp++;
    }

    public void MarkFirstSent(DateTimeOffset atUtc) => FirstSentAtUtc ??= atUtc;
}
