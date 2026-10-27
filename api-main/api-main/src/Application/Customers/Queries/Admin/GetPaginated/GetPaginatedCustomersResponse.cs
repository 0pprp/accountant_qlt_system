namespace Application.Customers.Queries.Admin.GetPaginated;

public record GetPaginatedCustomersResponse
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string BusinessName { get; set; }
    public required string BusinessAddress { get; set; }
    public string? OrderListName { get; set; }
    public int OrdersCount { get; set; }
    public DateOnly? LastInstallmentPaymentDate { get; set; }
    /// <summary>Arabic rating name only (ممتاز/جيد/ضعيف/متلكئ/سيء).</summary>
    public string? Rating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
