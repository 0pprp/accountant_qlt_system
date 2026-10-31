using Application.SalesWorkflow.Queries.App.GetBins;

namespace Application.SalesWorkflow.Queries.App.GetByStatus;

public record GetSalesWorkflowByStatusQuery : IRequest<Result<List<SalesWorkflowItemDto>>>
{
    public int UserId { get; set; }
    public required string Status { get; set; }
}
