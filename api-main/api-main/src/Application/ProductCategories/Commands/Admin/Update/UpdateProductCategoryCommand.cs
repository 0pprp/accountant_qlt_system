namespace Application.ProductCategories.Commands.Admin.Update;

public record UpdateProductCategoryCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public record UpdateProductCategoryDto(string Name);