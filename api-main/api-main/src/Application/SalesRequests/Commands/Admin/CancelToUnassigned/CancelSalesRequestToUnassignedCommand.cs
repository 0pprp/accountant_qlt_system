namespace Application.SalesRequests.Commands.Admin.CancelToUnassigned;

public record CancelSalesRequestToUnassignedCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public required string Reason { get; init; }
}

public class CancelSalesRequestToUnassignedCommandValidator : AbstractValidator<CancelSalesRequestToUnassignedCommand>
{
    public CancelSalesRequestToUnassignedCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("سبب الإلغاء مطلوب")
            .MaximumLength(500);
    }
}
