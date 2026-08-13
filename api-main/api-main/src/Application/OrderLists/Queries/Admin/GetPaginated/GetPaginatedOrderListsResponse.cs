using Application.Branches.Common;

namespace Application.OrderLists.Queries.Admin.GetPaginated;

public record GetPaginatedOrderListsResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required GetUserDto Mandob { get; set; }
    public required GetUserDto Motaba { get; set; }
    public required GetBranchDto Branch { get; set; }
    public int CustomersCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public record GetUserDto
{
    public int Id { get; set; }
    public required string FullName { get; set; }
}