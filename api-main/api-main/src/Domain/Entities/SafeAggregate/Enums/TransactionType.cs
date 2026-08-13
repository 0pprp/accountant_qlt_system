namespace Domain.Entities.SafeAggregate.Enums;

public enum TransactionType : byte
{
    SellerPayment = 0,
    Purchase = 1,
    Expense = 2,
    SafeTransfer = 3
}