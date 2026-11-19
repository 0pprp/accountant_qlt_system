namespace Domain.Entities.ArchiveAggregate.Enums;

/// <summary>
/// محطات سلسلة العهدة بترتيبها. تُستخدم وجهةً للاسترجاع ومُعرِّفاً لمرحلة المستلِم،
/// وليست حالة المعاملة — الحالة في <see cref="ArchiveTransactionStatus"/>.
/// </summary>
public enum ArchiveStage : byte
{
    SalesEmployee = 0,
    BranchManagement = 1,
    Follower = 2,
    Archive = 3
}
