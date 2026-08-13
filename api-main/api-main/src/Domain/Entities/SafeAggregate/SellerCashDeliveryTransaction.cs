using Domain.Common;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.SafeAggregate;

public class SellerCashDeliveryTransaction : Entity
{
    public SellerCashDeliveryTransaction(string? description, DateOnly? date, int sellerId)
    {
        Description = description;
        Date = date;
        SellerId = sellerId;
    }

    public SellerCashDeliveryTransaction(int sellerId, DateOnly date)
    {
        SellerId = sellerId;
        Date = date;
    }

    public string? Description { get; set; }
    public DateOnly? Date { get; set; }
    public int SellerId { get; set; }
    public int TransactionId { get; set; }

    public User? Seller { get; set; }
    public Transaction? Transaction { get; set; }
}