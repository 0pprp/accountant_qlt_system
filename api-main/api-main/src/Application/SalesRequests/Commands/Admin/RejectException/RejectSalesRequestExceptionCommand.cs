namespace Application.SalesRequests.Commands.Admin.RejectException;

public record RejectSalesRequestExceptionCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class RejectSalesRequestExceptionCommandValidator : AbstractValidator<RejectSalesRequestExceptionCommand>
{
    public RejectSalesRequestExceptionCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
