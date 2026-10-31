using Application.SalesWorkflow.Queries.App.GetBins;

namespace Application.SalesWorkflow.Commands.App.Transfer;

public record TransferSalesWorkflowCommand : IRequest<Result<SalesWorkflowItemDto>>
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public required string TargetStatus { get; set; }
    public string? Reason { get; set; }
}

public record TransferSalesWorkflowDto
{
    public required string TargetStatus { get; set; }
    public string? Reason { get; set; }
}
