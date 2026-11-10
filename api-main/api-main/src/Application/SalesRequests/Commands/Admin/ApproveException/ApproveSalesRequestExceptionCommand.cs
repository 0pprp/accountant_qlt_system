namespace Application.SalesRequests.Commands.Admin.ApproveException;

public record ApproveSalesRequestExceptionCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class ApproveSalesRequestExceptionCommandValidator : AbstractValidator<ApproveSalesRequestExceptionCommand>
{
    public ApproveSalesRequestExceptionCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
