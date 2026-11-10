using Application.ManagementTasks.Common;

namespace Application.ManagementTasks.Commands.Admin.Accept;

public record AcceptManagementTaskCommand : IRequest<Result<ManagementTaskActionResultDto>>
{
    public int TaskId { get; init; }
    public int ExpectedVersion { get; init; }
}

public class AcceptManagementTaskCommandValidator : AbstractValidator<AcceptManagementTaskCommand>
{
    public AcceptManagementTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
    }
}
