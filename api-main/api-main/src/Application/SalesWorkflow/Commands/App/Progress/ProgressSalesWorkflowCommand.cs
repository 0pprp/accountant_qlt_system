using Application.SalesWorkflow.Queries.App.GetBins;

namespace Application.SalesWorkflow.Commands.App.Progress;

public record ProgressSalesWorkflowCommand : IRequest<Result<SalesWorkflowItemDto>>
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
}
