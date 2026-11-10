using Application.ManagementTasks.Common;

namespace Application.ManagementTasks.Commands.Admin.Reject;

public record RejectManagementTaskCommand : IRequest<Result<ManagementTaskActionResultDto>>
{
    public int TaskId { get; init; }
    public int ExpectedVersion { get; init; }
    public string Note { get; init; } = string.Empty;
}

public class RejectManagementTaskCommandValidator : AbstractValidator<RejectManagementTaskCommand>
{
    public RejectManagementTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("ملاحظة الرفض مطلوبة.")
            .MaximumLength(1000).WithMessage("ملاحظة الرفض يجب ألا تتجاوز 1000 حرف");
    }
}
