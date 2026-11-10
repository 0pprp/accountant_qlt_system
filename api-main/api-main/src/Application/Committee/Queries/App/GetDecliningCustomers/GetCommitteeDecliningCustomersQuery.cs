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
    public string FromLabel { get; set; } = "";
    public string ToLabel { get; set; } = "";
    public int FromScore { get; set; }
    public int ToScore { get; set; }
    public int DaysBetween { get; set; }
    public DateOnly PriorCaptureDate { get; set; }
    public DateOnly CurrentCaptureDate { get; set; }

    /// <summary>توافق واجهة قديمة — يساوي DaysBetween وليس أيام تسديد فائتة.</summary>
    public int MissedDays { get; set; }

    /// <summary>توافق واجهة قديمة — يساوي ToLabel.</summary>
    public required string RiskLabel { get; set; }
}
