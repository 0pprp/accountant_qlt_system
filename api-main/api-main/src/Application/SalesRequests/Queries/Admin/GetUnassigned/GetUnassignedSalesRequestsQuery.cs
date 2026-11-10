using Application.Common.Models;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Queries.Admin.GetUnassigned;

public record GetUnassignedSalesRequestsQuery : IRequest<Result<PaginatedList<GetUnassignedSalesRequestItem>>>
{
    public required Pagination Pagination { get; init; }
    public GetUnassignedSalesRequestsFilter Filter { get; init; } = new();
}

public class GetUnassignedSalesRequestsFilter
{
    public int? BranchId { get; set; }
    public int? ProvinceId { get; set; }
    public SalesRequestSourceType? SourceType { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Search { get; set; }
}

public record GetUnassignedSalesRequestItem
{
    public int Id { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public SalesRequestSourceType SourceType { get; init; }
    public required string SourceTypeLabel { get; init; }
    public string? OriginalExternalId { get; init; }
    public int BranchId { get; init; }
    public string? BranchName { get; init; }
    public int? ProvinceId { get; init; }
    public string? ProvinceName { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public string? CreatedByName { get; init; }
    public string StatusLabel { get; init; } = "غير مسند";
}
