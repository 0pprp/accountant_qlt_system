namespace Application.Safes.Queries.Admin.GetSellers;

public record GetSafeSellersResponse
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public string? OrderListName { get; set; }
    public double DeliveredCashAmount { get; set; }
    public double UndeliveredCashAmount { get; set; }
    public DateTimeOffset? LastCashDeliveryDate { get; set; }
    public string? LastCashDeliveryDescription { get; set; }
}