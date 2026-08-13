namespace Application.Orders.Queries.Admin.GetInstallmentPayments;

public record GetOrderInstallmentPaymentsQuery(int OrderId) : IRequest<Result<List<GetOrderInstallmentPaymentsResponse>>>;