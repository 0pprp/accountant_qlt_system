using Application.ManagementTasks.Common;

namespace Application.ManagementTasks.Commands.Admin.Confirm;

public record ConfirmManagementTaskCommand : IRequest<Result<ManagementTaskActionResultDto>>
{
    public int TaskId { get; init; }
    public int ExpectedVersion { get; init; }
}

public class ConfirmManagementTaskCommandValidator : AbstractValidator<ConfirmManagementTaskCommand>
{
    public ConfirmManagementTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
    }
}
