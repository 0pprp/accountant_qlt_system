using Application.Common.Models.KeysetPagination;

namespace Application.InstallmentPayments.Queries.App.GetPaginated;

public record GetPaginatedInstallmentPaymentsQuery : IRequest<Result<List<GetPaginatedInstallmentPaymentsResponse>>>
{
    public required KeysetPagination<DateTimeOffset?> Pagination { get; set; }
    public required GetPaginatedInstallmentPaymentsFilter Filter { get; set; }
    public int UserId { get; set; }
}

public record GetPaginatedInstallmentPaymentsFilter
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? OrderListId { get; set; }
    public string? FullName { get; set; }
}