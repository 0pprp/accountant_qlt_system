namespace Application.Products.Common;

public record ProductDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}