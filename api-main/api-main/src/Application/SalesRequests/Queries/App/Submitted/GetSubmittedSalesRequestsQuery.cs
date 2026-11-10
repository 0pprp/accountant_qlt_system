using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Queries.App.Submitted;

public record GetSubmittedSalesRequestsQuery(SalesRequestSourceType Source, string Tab = "all", int Page = 1)
    : IRequest<Result<SubmittedSalesRequestsPage>>;
public record SubmittedSalesRequestsPage(List<SubmittedSalesRequestRow> Items, int Page, bool HasMore);
public record SubmittedSalesRequestRow(int RequestId, string CustomerName, string? CustomerPhone,
    string? Address, string? RequestedProduct, string? SubmissionNote, string StatusLabel,
    string? TargetEmployeeName, string? RejectionReason, string? PendingNote,
    DateTimeOffset? CreatedAt, string? BranchName);


