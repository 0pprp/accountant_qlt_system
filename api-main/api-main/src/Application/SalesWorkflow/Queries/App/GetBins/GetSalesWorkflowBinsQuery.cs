namespace Application.SalesWorkflow.Queries.App.GetBins;

public record GetSalesWorkflowBinsQuery(int UserId) : IRequest<Result<GetSalesWorkflowBinsResponse>>;

public record GetSalesWorkflowBinsResponse
{
    public required List<SalesWorkflowBinDto> Bins { get; set; }
}

public record SalesWorkflowBinDto
{
    public required string Status { get; set; }
    public int Count { get; set; }
    public required List<SalesWorkflowItemDto> Items { get; set; }
}

public record SalesWorkflowItemDto
{
    public int Id { get; set; }
    public required string Status { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Reason { get; set; }
    public required SalesWorkflowCustomerDto Customer { get; set; }
}

public record SalesWorkflowCustomerDto
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
}
