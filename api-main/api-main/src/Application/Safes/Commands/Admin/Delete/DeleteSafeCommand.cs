namespace Application.Safes.Commands.Admin.Delete;

public record DeleteSafeCommand(int Id) : IRequest<Result>;
