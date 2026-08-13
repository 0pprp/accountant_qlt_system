using Domain.Common;

namespace Domain.Entities.ProductAggregate;

public class ProductCategory : AuditableEntity
{
    public ProductCategory(string name)
    {
        Name = name;
        Products = new HashSet<Product>();
    }

    public string Name { get; set; }
    public ICollection<Product> Products { get; set; }

    public void Update(string name)
    {
        Name = name;
    }
}