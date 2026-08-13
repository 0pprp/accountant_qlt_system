namespace Application.ProductCategories.Commands.Admin.Delete;

public record DeleteProductCategoryCommand(int Id) : IRequest<Result>;