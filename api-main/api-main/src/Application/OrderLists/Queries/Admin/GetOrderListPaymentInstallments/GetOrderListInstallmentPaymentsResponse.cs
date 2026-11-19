namespace Application.OrderLists.Queries.Admin.GetOrderListPaymentInstallments;

public record GetOrderListInstallmentPaymentsResponse
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public required string CustomerFullName { get; set; }
    /// <summary>المندوب أو المتابع الذي أرسل التسديد من تطبيقه، وإلا مندوب القائمة.</summary>
    public string? SellerFullName { get; set; }
    public string? OrderListName { get; set; }
    public DateOnly Date { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}