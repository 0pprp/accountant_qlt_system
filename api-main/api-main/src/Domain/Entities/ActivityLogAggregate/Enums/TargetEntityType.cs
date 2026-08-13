namespace Domain.Entities.ActivityLogAggregate.Enums;

public enum TargetEntityType : byte
{
    User = 0,
    Branch = 1,
    Customer = 2,
    Order = 3,
    OrderList = 4,
    Product = 5,
    ProductCategory = 6,
    InstallmentPayment = 7,
    Purchase = 8,
    Expense = 9,
    Safe = 10,
    Transaction = 11,
    Attachment = 12,
    Notification = 13
}