namespace Application.SalesRequests.Commands.Admin.Restore;

public record RestoreSalesRequestCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public string? Reason { get; init; }
}

public class RestoreSalesRequestCommandValidator : AbstractValidator<RestoreSalesRequestCommand>
{
    public RestoreSalesRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(300);
    }
}
