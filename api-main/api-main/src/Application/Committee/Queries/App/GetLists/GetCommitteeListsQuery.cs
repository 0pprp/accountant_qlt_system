namespace Application.Committee.Queries.App.GetLists;

public record GetCommitteeListsQuery(int UserId) : IRequest<Result<List<CommitteeListDto>>>;

public record CommitteeListDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string MandobName { get; set; }
    public int BranchId { get; set; }
    public bool CollectionEnabled { get; set; }
}
