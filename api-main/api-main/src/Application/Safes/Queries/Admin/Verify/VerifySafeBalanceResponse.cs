namespace Application.Safes.Queries.Admin.Verify;

public record VerifySafeBalanceResponse
{
    public int SafeId { get; set; }
    public required string SafeName { get; set; }

    /// <summary>الرصيد المخزون في عمود Safe.Balance.</summary>
    public double StoredBalance { get; set; }

    /// <summary>مجموع المبالغ الموقّعة في SafeMovements.</summary>
    public double LedgerBalance { get; set; }

    public double Difference { get; set; }
    public int MovementsCount { get; set; }

    /// <summary>true يعني الرصيد مطابق للسجل تماماً.</summary>
    public bool IsBalanced { get; set; }

    /// <summary>عدد الحركات التي لا يطابق فيها BalanceAfter التسلسل التصاعدي.</summary>
    public int InconsistentSnapshots { get; set; }
}
