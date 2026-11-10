using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Commands.App.Submit;

public record SubmitSalesRequestCommand : IRequest<Result<SubmissionReceipt>>
{
    public SalesRequestSourceType Source { get; init; }
    public Guid SubmissionKey { get; init; }
    public int BranchId { get; init; }
    public string CustomerName { get; init; } = "";
    public string CustomerPhone { get; init; } = "";
    public string Address { get; init; } = "";
    public string RequestedProduct { get; init; } = "";
    public string? Note { get; init; }
}

public record SubmissionReceipt(int RequestId, string Status, DateTimeOffset? CreatedAt);
public record SubmissionBranch(int Id, string Name);
public record GetSubmissionBranchesQuery(SalesRequestSourceType Source) : IRequest<Result<List<SubmissionBranch>>>;
