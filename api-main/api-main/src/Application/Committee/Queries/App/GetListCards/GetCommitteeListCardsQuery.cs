namespace Application.Committee.Queries.App.GetListCards;

public record GetCommitteeListCardsQuery(int UserId) : IRequest<Result<List<CommitteeListCardDto>>>;

public record CommitteeListCardDto
{
    public int ListId { get; set; }
    public required string ListName { get; set; }
    public required string MandobName { get; set; }
    public required string PhantomLabel { get; set; }
    public double CurrentMonthScore { get; set; }
    public double PreviousMonthScore { get; set; }
    public int ActiveCustomers { get; set; }
    public bool CollectionEnabled { get; set; }
}
