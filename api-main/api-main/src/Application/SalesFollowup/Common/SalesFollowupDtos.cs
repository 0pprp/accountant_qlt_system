namespace Application.SalesFollowup.Common;

public class SalesFollowupCustomerFact
{
    public int CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? ShopName { get; set; }
    public string? Notes { get; set; }
    public double AmountTotalSales { get; set; }
    public double ReceiptsTotal { get; set; }
    public double AmountRemaining { get; set; }
    public double AmountDaySales { get; set; }
    public DateOnly? DateSaleDevice { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
    public double LastPaymentAmount { get; set; }
    public int CountReceiptDevice { get; set; }
    public int NumberOfDayDevice { get; set; }
    public double ReceiptRateDevice { get; set; }
    public int? Score { get; set; }
    public string RatingLabel { get; set; } = "—";
    public bool IsLegal { get; set; }
    public string? ItemsNames { get; set; }
    public string? SaleName { get; set; }
    public string? ReceiptName { get; set; }
    public int? OrderListId { get; set; }
    public string? ListName { get; set; }
}

public class SalesFollowupRatingRowDto
{
    public int Score { get; set; }
    public string? Note { get; set; }
    public DateTime RatingDate { get; set; }
    public string? ListName { get; set; }
    public string? CustomerName { get; set; }
    public int? ListId { get; set; }
    public int? RequestId { get; set; }
    public string? Kind { get; set; }
    public string? DelegateName { get; set; }
}

public class SalesFollowupRatingBucketDto
{
    public List<SalesFollowupRatingRowDto> High { get; set; } = [];
    public List<SalesFollowupRatingRowDto> Low { get; set; } = [];
}

public class SalesFollowupSystemDayAvgDto
{
    public DateTime RatingDate { get; set; }
    public double AverageScore { get; set; }
    public string RatingLabel { get; set; } = "—";
    public int CustomerCount { get; set; }
}

public class SalesFollowupEmployeeCardDto
{
    public int EmployeeId { get; set; }
    public required string EmployeeName { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int CustomerCount { get; set; }
    public double AverageScore { get; set; }
    public string RatingLabel { get; set; } = "—";
    public int? ManagerScore { get; set; }
    public string? ManagerNote { get; set; }
    public DateTimeOffset? ManagerRatedAt { get; set; }
}

public class SalesFollowupSalesByListDto
{
    public int? ListId { get; set; }
    public string ListName { get; set; } = "بدون قائمة";
    public int CustomerCount { get; set; }
    public double TotalSales { get; set; }
    public double TotalRemaining { get; set; }
}

public class SalesFollowupClassCountDto
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
}

public class SalesFollowupConversionFromDto
{
    public string FromLabel { get; set; } = "";
    public int Count { get; set; }
}

public class SalesFollowupConversionCustomerDto
{
    public int CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public string FromLabel { get; set; } = "";
}

public class SalesFollowupConversionTargetDayDto
{
    public DateOnly Date { get; set; }
    public int ArrivedCount { get; set; }
    public List<SalesFollowupConversionFromDto> FromBreakdown { get; set; } = [];
    public List<SalesFollowupConversionCustomerDto> Customers { get; set; } = [];
}

public class SalesFollowupConversionTargetDto
{
    public string TargetLabel { get; set; } = "";
    public int ArrivedCount { get; set; }
    public string DetailText { get; set; } = "";
    public List<SalesFollowupConversionFromDto> FromBreakdown { get; set; } = [];
    public List<SalesFollowupConversionCustomerDto> Customers { get; set; } = [];
    public List<SalesFollowupConversionTargetDayDto> Last30Days { get; set; } = [];
}

public class SalesFollowupConversionDto
{
    public int Improved { get; set; }
    public int Declined { get; set; }
    public int Unchanged { get; set; }
    public string SummaryText { get; set; } = "";
    public List<SalesFollowupConversionTargetDto> Targets { get; set; } = [];
}

public class SalesFollowupManagerRatingDto
{
    public int Score { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset RatedAt { get; set; }
    public string? RatedByUserName { get; set; }
}

public class SalesFollowupDetailDto
{
    public required SalesFollowupEmployeeCardDto Employee { get; set; }
    public SalesFollowupManagerRatingDto? ManagerRating { get; set; }
    public SalesFollowupRatingBucketDto FollowerSaleAuthRatings { get; set; } = new();
    public SalesFollowupRatingBucketDto CollectionDelegateRatings { get; set; } = new();
    public List<SalesFollowupSystemDayAvgDto> SystemRatingHistory { get; set; } = [];
    public int ZeroedCustomerCount { get; set; }
    public List<SalesFollowupCustomerFact> Customers { get; set; } = [];
    public List<SalesFollowupSalesByListDto> SalesByList { get; set; } = [];
    public List<SalesFollowupClassCountDto> ClassCounts { get; set; } = [];
    public SalesFollowupConversionDto Conversion { get; set; } = new();
    public bool IsAppBlocked { get; set; }
    public string Marker { get; set; } = SalesFollowupRules.DeployMarker;
}

public class SalesFollowupAppBlockDto
{
    public int EmployeeId { get; set; }
    public bool IsBlocked { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}
