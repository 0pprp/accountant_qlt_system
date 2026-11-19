namespace Application.SalesRequests.Commands.Admin.Return;

public record ReturnSalesRequestCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public required string Note { get; init; }
}

public class ReturnSalesRequestCommandValidator : AbstractValidator<ReturnSalesRequestCommand>
{
    public ReturnSalesRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("ملاحظة الإرجاع مطلوبة")
            .MaximumLength(500);
    }
}
