namespace Application.Common.Constants;

public static class AttachmentPath
{
    public static readonly string Root = "Attachments";
    public static string UsersSubfolder(int userId) => $"Users/{userId}";
    public static string OrdersSubfolder(int orderId) => $"Orders/{orderId}";
    public static string CustomersSubfolder(int customerId) => $"Customers/{customerId}";
    public static string PurchasesSubfolder(int purchaseId) => $"Purchases/{purchaseId}";
    public static string ExpensesSubfolder(int expenseId) => $"Expenses/{expenseId}";
    public static string ManagementTasksSubfolder(int taskId) => $"ManagementTasks/{taskId}";
    public static string LegalFollowupCaseSubfolder(int caseId) => $"LegalFollowups/{caseId}";
    public static string AnnouncementsSubfolder() => "Announcements";
    public static string BranchVisitsSubfolder(int visitId) => $"BranchVisits/{visitId}";
}