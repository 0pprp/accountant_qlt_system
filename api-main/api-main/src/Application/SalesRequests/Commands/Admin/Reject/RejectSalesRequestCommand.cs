namespace Application.SalesRequests.Commands.Admin.Reject;

public record RejectSalesRequestCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public string? Reason { get; init; }
}

public class RejectSalesRequestCommandValidator : AbstractValidator<RejectSalesRequestCommand>
{
    public RejectSalesRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
