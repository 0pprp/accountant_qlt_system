namespace Application.OrderLists.Queries.App.GetAll;

public record GetAllOrderListsResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string MandobFullName { get; set; }
    public required string MandobPhoneNumber { get; set; }
    public int TotalOrderCount { get; set; }
    public int TodayCollectedOrderCount { get; set; }
    public double TotalSellAmount { get; set; }
    public double TotalDailyInstallmentAmount { get; set; }
    public double TotalOverdueInstallmentAmount { get; set; }
    public double TotalCollectedInstallmentAmount { get; set; }
    public double TotalUnpaidInstallmentAmount { get; set; }
}