namespace Application.Customers.Queries.App.GetCustomerInstallmentPayments;

public record GetCustomerInstallmentPaymentsQuery : IRequest<Result<List<GetCustomerInstallmentPaymentsResponse>>>
{
    public int CustomerId { get; set; }
    public int UserId { get; set; }
}