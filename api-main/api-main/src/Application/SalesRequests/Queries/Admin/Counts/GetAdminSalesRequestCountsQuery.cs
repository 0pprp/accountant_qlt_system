using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Queries.Admin.Counts;

public record GetAdminSalesRequestCountsQuery : IRequest<Result<AdminSalesRequestCountsResponse>>
{
    public int? BranchId { get; init; }
    public int? ProvinceId { get; init; }
    public int? EmployeeId { get; init; }
    public SalesRequestSourceType? SourceType { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public string? Search { get; init; }
}

public record AdminSalesRequestCountsResponse
{
    public required IReadOnlyList<AdminSalesRequestTabCount> Tabs { get; init; }
    public IReadOnlyList<string> AllowedTabs { get; init; } = [];
    public IReadOnlyList<AdminSalesRequestExtraViewCount> ExtraViews { get; init; } = [];
    public IReadOnlyList<string> AllowedExtraViews { get; init; } = [];
    public IReadOnlyList<string> ExtraViewsNotImplemented { get; init; } = [];
}

public record AdminSalesRequestTabCount
{
    public required string Tab { get; init; }
    public required string Label { get; init; }
    public int Count { get; init; }
}

public record AdminSalesRequestExtraViewCount
{
    public required string Tab { get; init; }
    public required string Label { get; init; }
    public int Count { get; init; }
    public bool Overlapping { get; init; } = true;
    public bool IncludedInAll { get; init; }
    public string? Caveat { get; init; }
}
