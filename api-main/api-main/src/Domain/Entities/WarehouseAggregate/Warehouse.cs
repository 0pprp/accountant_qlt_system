using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.ProductAggregate;

namespace Domain.Entities.WarehouseAggregate;

public class Warehouse : Entity
{
    public Warehouse(string name, int branchId)
    {
        Name = name;
        BranchId = branchId;
        Products = new HashSet<Product>();
    }

    public Warehouse(string name)
    {
        Name = name;
        Products = new HashSet<Product>();
    }

    public string Name { get; set; }
    public int BranchId { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Product> Products { get; set; }
}