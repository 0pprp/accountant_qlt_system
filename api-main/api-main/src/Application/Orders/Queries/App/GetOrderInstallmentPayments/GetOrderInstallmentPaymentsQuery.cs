namespace Application.Orders.Queries.App.GetOrderInstallmentPayments;

public record GetOrderInstallmentPaymentsQuery : IRequest<Result<GetOrderInstallmentPaymentsResponse>>
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
}