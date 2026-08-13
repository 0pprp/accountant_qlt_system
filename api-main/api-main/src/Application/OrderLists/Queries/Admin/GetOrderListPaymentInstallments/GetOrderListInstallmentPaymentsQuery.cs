using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.OrderLists.Queries.Admin.GetOrderListPaymentInstallments;

public record GetOrderListInstallmentPaymentsQuery : IRequest<Result<PaginatedList<GetOrderListInstallmentPaymentsResponse>>>
{
    public int OrderListId { get; set; }
    public required Pagination Pagination { get; set; }
    public required GetOrderListInstallmentPaymentsFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetOrderListInstallmentPaymentsFilter
{
    public int BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}