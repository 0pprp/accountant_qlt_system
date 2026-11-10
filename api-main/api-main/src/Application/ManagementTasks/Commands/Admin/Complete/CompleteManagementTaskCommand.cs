using Application.ManagementTasks.Common;

namespace Application.ManagementTasks.Commands.Admin.Complete;

public record CompleteManagementTaskCommand : IRequest<Result<ManagementTaskActionResultDto>>
{
    public int TaskId { get; init; }
    public int ExpectedVersion { get; init; }
}

public class CompleteManagementTaskCommandValidator : AbstractValidator<CompleteManagementTaskCommand>
{
    public CompleteManagementTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
    }
}
