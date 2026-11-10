namespace Application.SalesRequests.Commands.Admin.RequestException;

public record RequestSalesRequestExceptionCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public string? Reason { get; init; }
}

public class RequestSalesRequestExceptionCommandValidator : AbstractValidator<RequestSalesRequestExceptionCommand>
{
    public RequestSalesRequestExceptionCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
