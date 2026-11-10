namespace Application.SalesRequests.Commands.Admin.SoftDelete;

public record SoftDeleteSalesRequestCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class SoftDeleteSalesRequestCommandValidator : AbstractValidator<SoftDeleteSalesRequestCommand>
{
    public SoftDeleteSalesRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
