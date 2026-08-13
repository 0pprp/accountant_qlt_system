namespace Application.Branches.Commands.Admin.Delete;

public record DeleteBranchCommand(int Id) : IRequest<Result>;