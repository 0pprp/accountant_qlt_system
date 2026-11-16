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
    /// <summary>زبائن لديهم GPS محفوظ (بروفايل أو طلب نشط).</summary>
    public int LocatedCustomers { get; set; }
    /// <summary>إجمالي زبائن القائمة قيد التنفيذ لحساب عداد المواقع.</summary>
    public int LocationCustomersTotal { get; set; }
}
