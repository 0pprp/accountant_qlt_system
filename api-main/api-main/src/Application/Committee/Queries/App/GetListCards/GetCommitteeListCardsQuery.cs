namespace Application.Committee.Queries.App.GetListCards;

public record GetCommitteeListCardsQuery(int UserId, int? ListId = null) : IRequest<Result<List<CommitteeListCardDto>>>;

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
    public IReadOnlyList<CommitteeLocationCustomer>? LocationCustomers { get; set; }
    public int LocatedCustomers { get; set; }
    public int PhotographedCustomers { get; set; }
    public int CompletedCustomers { get; set; }
    public int LocationCustomersTotal { get; set; }
}

public record CommitteeLocationCustomer(int CustomerId, string FullName, string Phone, bool HasLocation, bool HasPhoto, string? ShopName);
