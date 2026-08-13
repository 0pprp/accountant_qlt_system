namespace Application.OrderLists.Commands.Admin.Update;

public record UpdateOrderListCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int MandobId { get; set; }
    public int MotabaId { get; set; }
    public int BranchId { get; set; }
}

public record UpdateOrderListDto
{
    public required string Name { get; set; }
    public int MandobId { get; set; }
    public int MotabaId { get; set; }
    public int BranchId { get; set; }
}