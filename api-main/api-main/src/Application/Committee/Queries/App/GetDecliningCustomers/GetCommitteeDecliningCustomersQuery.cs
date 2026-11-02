namespace Application.Committee.Queries.App.GetDecliningCustomers;

public record GetCommitteeDecliningCustomersQuery(int UserId)
    : IRequest<Result<List<CommitteeDecliningCustomerDto>>>;

public record CommitteeDecliningCustomerDto
{
    public int CustomerId { get; set; }
    public int ListId { get; set; }
    public required string ListName { get; set; }
    public required string FullName { get; set; }
    public string? Phone { get; set; }
    public int MissedDays { get; set; }
    public required string RiskLabel { get; set; }
}
