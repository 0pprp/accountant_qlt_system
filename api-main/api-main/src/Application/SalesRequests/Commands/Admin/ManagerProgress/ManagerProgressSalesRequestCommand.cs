using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Commands.Admin.ManagerProgress;

public enum ManagerProgressAction
{
    Prepare,
    Pend
}

public record ManagerProgressSalesRequestCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public ManagerProgressAction Action { get; init; }
    public string? Note { get; init; }
}

public class ManagerProgressSalesRequestCommandValidator : AbstractValidator<ManagerProgressSalesRequestCommand>
{
    public ManagerProgressSalesRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Action).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(500);
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("سبب التعليق مطلوب")
            .When(x => x.Action == ManagerProgressAction.Pend);
    }
}

public static class ManagerProgressRules
{
    public static bool IsAllowed(ManagerProgressAction action, SalesRequestStatus status)
        => action switch
        {
            ManagerProgressAction.Prepare => status is SalesRequestStatus.Inspected or SalesRequestStatus.Pending,
            ManagerProgressAction.Pend => status is SalesRequestStatus.Inspected or SalesRequestStatus.PreparedForSale,
            _ => false
        };
}
