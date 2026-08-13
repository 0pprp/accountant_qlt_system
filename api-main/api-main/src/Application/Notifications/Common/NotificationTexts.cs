namespace Application.Notifications.Common;

public static class NotificationTexts
{
    public static class OrderCreated
    {
        public const string Title = "طلب جديد من التطبيق";

        public static string Description(string customerFullName, double totalSellAmount) =>
            $"تم إنشاء طلب جديد للعميل {customerFullName} بمبلغ {totalSellAmount}";
    }

    public static class TransactionCreated
    {
        public const string Title = "معاملة جديدة في خزنة الفرع";

        public static string Description(string transactionType, string direction, double amount) =>
            $"معاملة {transactionType} ({direction}) بمبلغ {amount}";
    }
}
