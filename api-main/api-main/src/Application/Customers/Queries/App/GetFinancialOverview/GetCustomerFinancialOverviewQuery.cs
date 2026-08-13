namespace Application.Customers.Queries.App.GetFinancialOverview;

public record GetCustomerFinancialOverviewQuery : IRequest<Result<GetCustomerFinancialOverviewResponse>>
{
    public int CustomerId { get; set; }
    public int UserId { get; set; }
}