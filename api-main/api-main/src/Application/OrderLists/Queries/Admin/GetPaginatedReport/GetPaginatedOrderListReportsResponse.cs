using Application.Common.Models;
using Application.OrderLists.Queries.Admin.GetPaginated;

namespace Application.OrderLists.Queries.Admin.GetPaginatedReport;

public record GetPaginatedOrderListReportsResponse
{
    public required PaginatedList<GetPaginatedOrderListReportItem> Items { get; set; }
    public int TotalInstallmentPaymentCount { get; set; }
    public double TotalCollectedAmount { get; set; }
}

public record GetPaginatedOrderListReportItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int InstallmentPaymentsCount { get; set; }
    public double TotalPaidInstallmentsAmount { get; set; }
    public required GetUserDto Mandob { get; set; }
    public int CustomersCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}