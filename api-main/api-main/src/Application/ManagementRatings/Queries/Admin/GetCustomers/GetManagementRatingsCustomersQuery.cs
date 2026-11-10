using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetCustomers;

public record GetManagementRatingsCustomersQuery : IRequest<Result<ManagementRatingsCustomersResponse>>
{
    public int BranchId { get; init; }
    public string Scope { get; init; } = "branch";
    public int ResourceId { get; init; }
    public string Level { get; init; } = "";
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? Q { get; init; }
}
