using Application.SalesFollowup.Common;

namespace Application.SalesFollowup.Commands.Admin.UpsertManagerRating;

public record UpsertSalesFollowupManagerRatingCommand : IRequest<Result<SalesFollowupManagerRatingDto>>
{
    public int EmployeeId { get; init; }
    public int BranchId { get; init; }
    public int Score { get; init; }
    public string? Note { get; init; }
}
