namespace Application.SalesRequests.Commands.Admin.AssignBatch;

public record AssignSalesRequestBatchCommand : IRequest<Result<AssignSalesRequestBatchResponse>>
{
    public const int MaxItems = 50;

    public required IReadOnlyList<int> RequestIds { get; init; }
    public int EmployeeId { get; init; }
}

public sealed record AssignSalesRequestBatchItem(int RequestId, bool Succeeded, string? ErrorCode, string? ErrorMessage);

public sealed record AssignSalesRequestBatchResponse(int Succeeded, int Failed, IReadOnlyList<AssignSalesRequestBatchItem> Items);

public class AssignSalesRequestBatchCommandValidator : AbstractValidator<AssignSalesRequestBatchCommand>
{
    public AssignSalesRequestBatchCommandValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.RequestIds)
            .NotEmpty().WithMessage("اختر طلباً واحداً على الأقل")
            .Must(ids => ids.Count <= AssignSalesRequestBatchCommand.MaxItems)
            .WithMessage($"الحد الأعلى {AssignSalesRequestBatchCommand.MaxItems} طلباً في المرة الواحدة")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("أرقام الطلبات مكررة");
        RuleForEach(x => x.RequestIds).GreaterThan(0);
    }
}
