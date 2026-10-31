using Application.SalesWorkflow.Queries.App.GetBins;

namespace Application.SalesWorkflow.Commands.App.Complete;

public record CompleteSalesWorkflowCommand : IRequest<Result<SalesWorkflowItemDto>>
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
}
