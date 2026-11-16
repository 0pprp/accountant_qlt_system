namespace Application.Customers.Queries.App.GetCollectionBook;

public record GetCollectionBookQuery(int UserId) : IRequest<Result<GetCollectionBookResponse>>;

public record GetCollectionBookResponse
{
    public int? ListId { get; set; }
    public string? ListName { get; set; }
    public double AverageScore { get; set; }
    public string AverageLabel { get; set; } = "ضعيف";
    public double PreviousWeekScore { get; set; }
    public int RatedCustomerCount { get; set; }
    public double InstallmentSum { get; set; }
    public List<CollectionBookCustomerDto> Customers { get; set; } = [];
}

public record CollectionBookCustomerDto
{
    public int CustomerId { get; set; }
    public required string FullName { get; set; }
    public string? BusinessName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public double AmountTotalSales { get; set; }
    public double ReceiptsTotal { get; set; }
    public double AmountRemaining { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public DateOnly? SaleDate { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
    public int? ActiveOrderId { get; set; }
    public bool IsLegal { get; set; }
    public int? Score { get; set; }
    public string? RatingLabel { get; set; }

    /// تسديدات آخر 7 أيام: Amount1 = اليوم … Amount7 = قبل 6 أيام (تقويم العراق).
    public double Amount1 { get; set; }
    public double Amount2 { get; set; }
    public double Amount3 { get; set; }
    public double Amount4 { get; set; }
    public double Amount5 { get; set; }
    public double Amount6 { get; set; }
    public double Amount7 { get; set; }
}
