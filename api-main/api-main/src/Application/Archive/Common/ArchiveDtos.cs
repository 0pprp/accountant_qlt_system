using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Archive.Common;

public record ArchiveBundleListItemDto
{
    public int Id { get; init; }
    public int BranchId { get; init; }
    public string? BranchName { get; init; }
    public int SalesEmployeeUserId { get; init; }
    public required string SalesEmployeeName { get; init; }

    public int PeriodYear { get; init; }
    public int PeriodMonth { get; init; }
    public byte PeriodIndex { get; init; }
    public DateOnly PeriodStart { get; init; }
    public DateOnly PeriodEnd { get; init; }
    public required string PeriodLabel { get; init; }

    /// <summary>العشرة انتهت فصار الإرسال ممكناً.</summary>
    public bool IsPeriodClosed { get; init; }

    public bool CanSend { get; init; }
    public int PendingSendCount { get; init; }

    public int ItemCount { get; init; }
    public int WithSalesEmployeeCount { get; init; }
    public int SentToBranchCount { get; init; }
    public int AtBranchCount { get; init; }
    public int SentToFollowerCount { get; init; }
    public int AtFollowerCount { get; init; }
    public int SentToArchiveCount { get; init; }
    public int ArchivedCount { get; init; }
    public int VoidedCount { get; init; }
    public int OpenReturnCount { get; init; }

    /// <summary>«تم استلام x من y» التي يراها المُرسِل: كم استلم منه الطرف التالي.</summary>
    public int ReceivedOnwardCount { get; init; }

    /// <summary>وكم استلمتُ أنا من الوارد — رقم آخر، للمستلِم لا للمُرسِل.</summary>
    public int ReceivedByMeCount { get; init; }

    public DateTimeOffset? FirstSentAtUtc { get; init; }
    public DateTimeOffset? LastArchivedAtUtc { get; init; }
    public int ConcurrencyStamp { get; init; }
}

public record ArchiveTransactionDto
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public int CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public DateOnly PeriodSourceDate { get; init; }
    public ArchiveTransactionStatus Status { get; init; }
    public required string StatusLabel { get; init; }
    public ArchiveStage HoldingStage { get; init; }
    public bool IsInTransit { get; init; }
    public bool IsVoided { get; init; }
    public string? VoidReason { get; init; }
    public bool HasOpenReturn { get; init; }
    public int ReturnCount { get; init; }
    public string? LastReturnReason { get; init; }
    public int? FollowerUserId { get; init; }
    public string? FollowerName { get; init; }
    public int ConcurrencyStamp { get; init; }

    public DateTimeOffset? SentToBranchAtUtc { get; init; }
    public DateTimeOffset? ReceivedByBranchAtUtc { get; init; }
    public DateTimeOffset? SentToFollowerAtUtc { get; init; }
    public DateTimeOffset? ReceivedByFollowerAtUtc { get; init; }
    public DateTimeOffset? SentToArchiveAtUtc { get; init; }
    public DateTimeOffset? ArchivedAtUtc { get; init; }

    /// <summary>هل ينتظر استلام الطرف الذي يسأل الآن.</summary>
    public bool AwaitsMyReceipt { get; init; }
}

public record ArchiveBundleDetailDto
{
    public required ArchiveBundleListItemDto Bundle { get; init; }

    /// <summary>غير المستلمة تتصدّر القائمة بقرار المالك.</summary>
    public required List<ArchiveTransactionDto> Transactions { get; init; }

    public ArchiveStage? MyStage { get; init; }
    public bool CanSend { get; init; }
    public bool CanReceive { get; init; }
    public bool CanReturn { get; init; }
    public bool CanUndoSend { get; init; }
}

public record ArchiveActionResultDto
{
    public int BundleId { get; init; }
    public int AffectedCount { get; init; }
    public int SkippedCount { get; init; }
    public required string Message { get; init; }
}

public record ArchiveFlowRowDto
{
    public int BundleId { get; init; }
    public required string SalesEmployeeName { get; init; }
    public string? BranchName { get; init; }
    public required string PeriodLabel { get; init; }
    public int ItemCount { get; init; }
    public int ArchivedCount { get; init; }
    public int OpenReturnCount { get; init; }
    public required string WhereLabel { get; init; }
    public ArchiveStage? StuckAtStage { get; init; }
    public int? DaysSinceFirstSend { get; init; }
    public double? AverageHoursSalesToBranch { get; init; }
    public double? AverageHoursBranchToFollower { get; init; }
    public double? AverageHoursFollowerToArchive { get; init; }
}

public record ArchiveTransactionEventDto
{
    public ArchiveEventType EventType { get; init; }
    public required string EventLabel { get; init; }
    public ArchiveTransactionStatus PreviousStatus { get; init; }
    public ArchiveTransactionStatus NewStatus { get; init; }
    public required string ActorName { get; init; }
    public DateTimeOffset AtUtc { get; init; }
    public string? Note { get; init; }
}
