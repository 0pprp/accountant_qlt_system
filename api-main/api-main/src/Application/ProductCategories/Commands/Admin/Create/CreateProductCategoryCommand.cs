namespace Application.ProductCategories.Commands.Admin.Create;

public record CreateProductCategoryCommand(string Name) : IRequest<Result<CreateProductCategoryResponse>>;