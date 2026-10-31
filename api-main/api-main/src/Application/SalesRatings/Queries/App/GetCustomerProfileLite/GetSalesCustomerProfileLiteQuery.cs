namespace Application.SalesRatings.Queries.App.GetCustomerProfileLite;

public record GetSalesCustomerProfileLiteQuery : IRequest<Result<GetSalesCustomerProfileLiteResponse>>
{
    public int UserId { get; set; }
    public int CustomerId { get; set; }
}

public record GetSalesCustomerProfileLiteResponse
{
    public int CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? NationalCardNumber { get; set; }
    public string? RatingLabel { get; set; }
    public int? Score { get; set; }
    public bool? IsSettled { get; set; }
    public SalesPaymentSummaryDto? PaymentSummary { get; set; }
    public required List<SalesCustomerPaymentDto> Payments { get; set; }
}

public record SalesPaymentSummaryDto
{
    public double TotalSales { get; set; }
    public double TotalPaid { get; set; }
    public double Remaining { get; set; }
}

public record SalesCustomerPaymentDto
{
    public int Id { get; set; }
    public double Amount { get; set; }
    public DateOnly Date { get; set; }
}
