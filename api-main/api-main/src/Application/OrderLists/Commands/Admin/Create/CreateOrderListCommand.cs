namespace Application.OrderLists.Commands.Admin.Create;

public record CreateOrderListCommand : IRequest<Result>
{
    public required string Name { get; set; }
    public int MandobId { get; set; }
    public int MotabaId { get; set; }
    public int BranchId { get; set; }
}