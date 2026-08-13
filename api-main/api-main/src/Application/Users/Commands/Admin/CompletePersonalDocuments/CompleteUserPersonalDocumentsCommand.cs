namespace Application.Users.Commands.Admin.CompletePersonalDocuments;

public record CompleteUserPersonalDocumentsCommand(int UserId) : IRequest<Result>;