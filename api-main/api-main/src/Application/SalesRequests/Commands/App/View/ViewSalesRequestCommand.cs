namespace Application.SalesRequests.Commands.App.View;

public record ViewSalesRequestCommand(int RequestId) : IRequest<Result>;
