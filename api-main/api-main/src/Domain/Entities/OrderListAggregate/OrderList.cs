using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.OrderListAggregate;

public class OrderList : AuditableEntity
{
    public OrderList(string name, int mandobId, int motabaId, int branchId)
    {
        Name = name;
        MandobId = mandobId;
        MotabaId = motabaId;
        BranchId = branchId;
        Orders = new HashSet<Order>();
    }

    public string Name { get; set; }
    public int MandobId { get; set; }
    public int MotabaId { get; set; }
    public int BranchId { get; set; }

    public Branch? Branch { get; set; }
    public User? Mandob { get; set; }
    public User? Motaba { get; set; }
    public ICollection<Order> Orders { get; set; }

    public void Update(string name, int mandobId, int motabaId, int branchId)
    {
        Name = name;
        MandobId = mandobId;
        MotabaId = motabaId;
        BranchId = branchId;
    }
}