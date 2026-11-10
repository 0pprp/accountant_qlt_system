using Application.Common.Models;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Queries.Admin.GetDuplicateRequests;

public record GetDuplicateSalesRequestsQuery : IRequest<Result<PaginatedList<GetDuplicateSalesRequestItem>>>
{
    public int RequestId { get; init; }
    public required Pagination Pagination { get; init; }
}

public record GetDuplicateSalesRequestItem
{
    public int Id { get; init; }
    public SalesRequestSourceType SourceType { get; init; }
    public required string SourceTypeLabel { get; init; }
    public required string StatusLabel { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public string? AssignedEmployeeName { get; init; }
}
