using Application.ManagementTasks.Common;

namespace Application.ManagementTasks.Commands.Admin.Transfer;

public record TransferManagementTaskCommand : IRequest<Result<ManagementTaskActionResultDto>>
{
    public int TaskId { get; init; }
    public int ExpectedVersion { get; init; }
    public int TargetUserId { get; init; }
    public string? Note { get; init; }
}

public class TransferManagementTaskCommandValidator : AbstractValidator<TransferManagementTaskCommand>
{
    public TransferManagementTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TargetUserId)
            .GreaterThan(0).WithMessage("مستخدم الفرع مطلوب.");
        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("الملاحظة يجب ألا تتجاوز 1000 حرف");
    }
}
