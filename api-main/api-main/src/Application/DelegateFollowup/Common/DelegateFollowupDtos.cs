namespace Application.DelegateFollowup.Common;

public class DelegateFollowupUnresolvedFieldDto
{
    public required string Field { get; set; }
    public required string Note { get; set; }
}

public class DelegateFollowupListCardDto
{
    public int ListId { get; set; }
    public required string ListName { get; set; }
    public int BranchId { get; set; }
    public string? DelegateName { get; set; }
    public string? FollowerName { get; set; }
    public double RealRating { get; set; }
    public required string RealRatingLabel { get; set; }
    public int NonZeroedCustomerCount { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
}

public class DelegateFollowupDayAmountDto
{
    public DateOnly Date { get; set; }
    public double Amount { get; set; }
}

public class DelegateFollowupDayRatingDto
{
    public DateOnly Date { get; set; }
    public double Score { get; set; }
    public string Label { get; set; } = "";
    public int? BoardOperatorScore { get; set; }
    public string? BoardOperatorNote { get; set; }
}

public class DelegateFollowupTierMoneyDto
{
    public required string Label { get; set; }
    public bool RequiredDueAvailable { get; set; }
    public double? RequiredDue { get; set; }
    public double CollectedYesterday { get; set; }
    public List<DelegateFollowupDayAmountDto> CollectedLast30Days { get; set; } = [];
}

public class DelegateFollowupConversionFromDto
{
    public required string FromLabel { get; set; }
    public int Count { get; set; }
}

public class DelegateFollowupConversionCustomerDto
{
    public int CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public required string FromLabel { get; set; }
}

public class DelegateFollowupConversionTargetDayDto
{
    public DateOnly Date { get; set; }
    public int ArrivedCount { get; set; }
    public List<DelegateFollowupConversionFromDto> FromBreakdown { get; set; } = [];
    public List<DelegateFollowupConversionCustomerDto> Customers { get; set; } = [];
}

public class DelegateFollowupConversionTargetDto
{
    public required string TargetLabel { get; set; }
    public int ArrivedCount { get; set; }
    public string DetailText { get; set; } = "";
    public List<DelegateFollowupConversionFromDto> FromBreakdown { get; set; } = [];
    public List<DelegateFollowupConversionCustomerDto> Customers { get; set; } = [];
    public List<DelegateFollowupConversionTargetDayDto> Last30Days { get; set; } = [];
}

public class DelegateFollowupConversionDto
{
    public int Improved { get; set; }
    public int Declined { get; set; }
    public int Unchanged { get; set; }
    public string SummaryText { get; set; } = "";
    public bool LegalTargetAvailable { get; set; }
    public List<DelegateFollowupConversionTargetDto> Targets { get; set; } = [];
}

public class DelegateFollowupBoardRatingDto
{
    public int ListId { get; set; }
    public DateOnly RatingDate { get; set; }
    public int? Score { get; set; }
    public string? Note { get; set; }
    public string? RatedByUserName { get; set; }
    public DateTimeOffset? RatedAtUtc { get; set; }
    public bool HasRating { get; set; }
}

public class DelegateFollowupCommitteeRatingDto
{
    public bool Available { get; set; }
    public string Note { get; set; } = DelegateFollowupRules.UnresolvedNote;
    public DelegateFollowupBoardRatingDto? Latest { get; set; }
    public List<DelegateFollowupDayRatingDto> Last30Days { get; set; } = [];
}

public class DelegateFollowupSummaryDto
{
    public int ListId { get; set; }
    public required string ListName { get; set; }
    public int BranchId { get; set; }
    public DateOnly AsOfDate { get; set; }

    public double RealRating { get; set; }
    public required string RealRatingLabel { get; set; }
    public double RealRatingWeekAgo { get; set; }
    public required string RatingTrend { get; set; }

    public int NonZeroedCustomerCount { get; set; }

    public bool LegalAvailable { get; set; }
    public int? LegalCustomerCount { get; set; }

    public int ExcellentCount { get; set; }
    public int GoodCount { get; set; }
    public int WeakCount { get; set; }
    public int LaggingCount { get; set; }
    public int BadCount { get; set; }

    public int RemainingLt100Count { get; set; }
    public int Remaining100To200Count { get; set; }
    public int RemainingGe100Count { get; set; }

    public int NewSaleUnpaid3DaysCount { get; set; }

    /// <summary>false إن تعذّر حساب القسط المطلوب بشكل صحيح — لا يُعرض رقم بديل.</summary>
    public bool RequiredDueAvailable { get; set; }
    public double? RequiredDueToday { get; set; }
    public string? RequiredDueUnavailableReason { get; set; }
    public double CollectedYesterday { get; set; }
    public double CollectedLastFriday { get; set; }
    public DateOnly LastFridayDate { get; set; }
    public List<DelegateFollowupDayAmountDto> CollectedLast30Days { get; set; } = [];
    public List<DelegateFollowupDayAmountDto> CollectedLast30Fridays { get; set; } = [];
    public List<DelegateFollowupTierMoneyDto> TierMoney { get; set; } = [];

    public List<DelegateFollowupDayRatingDto> RealRatingLast30Days { get; set; } = [];
    public List<DelegateFollowupDayRatingDto> BoardOperatorLast30Days { get; set; } = [];
    public required DelegateFollowupCommitteeRatingDto FollowerCommittee { get; set; }
    public required DelegateFollowupCommitteeRatingDto SupervisionCommittee { get; set; }
    public required DelegateFollowupCommitteeRatingDto SalesDelegate { get; set; }

    public required DelegateFollowupConversionDto Conversion { get; set; }
    public required DelegateFollowupBoardRatingDto BoardOperatorRating { get; set; }

    public List<DelegateFollowupUnresolvedFieldDto> UnresolvedNotes { get; set; } = [];
}

public class DelegateFollowupCustomerDto
{
    public int CustomerId { get; set; }
    public int ListId { get; set; }
    public required string CustomerName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ShopName { get; set; }
    public string? SaleName { get; set; }
    public string? DelegateName { get; set; }
    public string? FollowerName { get; set; }
    public DateOnly? DateSaleDevice { get; set; }
    public double AmountTotalSales { get; set; }
    public double AmountDaySales { get; set; }
    public double ReceiptsTotal { get; set; }
    public double AmountRemaining { get; set; }
    public string? ItemsNames { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
    public double? LastPaymentAmount { get; set; }
    public int CountReceiptDevice { get; set; }
    public int NumberOfDayDevice { get; set; }
    public double ReceiptRateDevice { get; set; }
    public int? Score { get; set; }
    public string RealRatingLabel { get; set; } = "";
    public bool IsLegal { get; set; }
    public bool IsNewSaleUnpaid3Days { get; set; }
    public string? Notes { get; set; }
    public string? ReceiptName { get; set; }
}

public class DelegateFollowupPaymentDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateOnly Date { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
}

public class DelegateFollowupCustomerPaymentsDto
{
    public int CustomerId { get; set; }
    public int ListId { get; set; }
    public int BranchId { get; set; }
    public List<DelegateFollowupPaymentDto> Payments { get; set; } = [];
    public double TotalAmount { get; set; }
}

public class DelegateFollowupCustomersDto
{
    public int ListId { get; set; }
    public int BranchId { get; set; }
    public bool LegalAvailable { get; set; }
    public int NewSaleUnpaid3DaysCount { get; set; }
    public List<DelegateFollowupCustomerDto> Customers { get; set; } = [];
    public List<DelegateFollowupUnresolvedFieldDto> UnresolvedNotes { get; set; } = [];
}

public class DelegateFollowupActionResultDto
{
    public bool Created { get; set; }
    public required DelegateFollowupBoardRatingDto Rating { get; set; }
}
