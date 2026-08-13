using Domain.Common;
using Domain.Entities.BranchAggregate;

namespace Domain.Entities.ProvinceAggregate;

public class Province : Entity
{
    public Province(string name)
    {
        Name = name;
        Branches = new HashSet<Branch>();
    }

    public string Name { get; set; }
    public ICollection<Branch> Branches { get; set; }
}