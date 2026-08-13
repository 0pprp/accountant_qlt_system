namespace Application.Products.Commands.Admin.Delete;

public record DeleteProductCommand(int Id) : IRequest<Result>;