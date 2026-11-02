namespace Application.Committee.Queries.App.GetCollectionLists;

public record GetCommitteeCollectionListsQuery(int UserId)
    : IRequest<Result<List<CommitteeCollectionListDto>>>;

public record CommitteeCollectionListDto
{
    public int ListId { get; set; }
    public required string ListName { get; set; }
    public required string MandobName { get; set; }
    public int ActiveCustomers { get; set; }
}
