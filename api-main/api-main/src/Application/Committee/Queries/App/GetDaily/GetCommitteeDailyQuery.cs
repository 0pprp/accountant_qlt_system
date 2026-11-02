namespace Application.Committee.Queries.App.GetDaily;

public record GetCommitteeDailyQuery(
    int UserId,
    int ListId,
    DateOnly Date,
    string ShowType) : IRequest<Result<CommitteeDailyResponse>>;

public record CommitteeDailyResponse
{
    public int ListId { get; set; }
    public required string ListName { get; set; }
    public required string ShowType { get; set; }
    public DateOnly Date { get; set; }
    public List<CommitteeDailyCustomerDto> Customers { get; set; } = [];
}

public record CommitteeDailyCustomerDto
{
    public int CustomerId { get; set; }
    public required string FullName { get; set; }
    public string? Phone { get; set; }
    public double DailyInstallment { get; set; }
    public double PaidToday { get; set; }
    public bool IsPaid { get; set; }
}
