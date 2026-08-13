namespace Application.Common.Constants;

public static class AttachmentPath
{
    public static readonly string Root = "Attachments";
    public static string UsersSubfolder(int userId) => $"Users/{userId}";
    public static string OrdersSubfolder(int orderId) => $"Orders/{orderId}";
    public static string CustomersSubfolder(int customerId) => $"Customers/{customerId}";
    public static string PurchasesSubfolder(int purchaseId) => $"Purchases/{purchaseId}";
    public static string ExpensesSubfolder(int expenseId) => $"Expenses/{expenseId}";
}