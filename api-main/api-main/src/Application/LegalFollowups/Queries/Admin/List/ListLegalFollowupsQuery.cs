using Application.LegalFollowups.Common;

namespace Application.LegalFollowups.Queries.Admin.List;

public sealed class ListLegalFollowupsQuery : IRequest<Result<ListLegalFollowupsResponse>>
{
    public int? BranchId { get; init; }
    public string? Status { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
