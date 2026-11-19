namespace Application.SalesRequests.Commands.Admin.TransferBranch;

public record TransferSalesRequestBranchCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public int TargetBranchId { get; init; }
    public int? EmployeeId { get; init; }
    public required string Reason { get; init; }
}

public class TransferSalesRequestBranchCommandValidator : AbstractValidator<TransferSalesRequestBranchCommand>
{
    public TransferSalesRequestBranchCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.TargetBranchId).GreaterThan(0);
        RuleFor(x => x.EmployeeId).GreaterThan(0).When(x => x.EmployeeId.HasValue);
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("سبب النقل مطلوب")
            .MaximumLength(500);
    }
}
