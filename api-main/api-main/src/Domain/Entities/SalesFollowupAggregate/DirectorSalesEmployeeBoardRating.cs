using Domain.Common;

namespace Domain.Entities.SalesFollowupAggregate;

public class DirectorSalesEmployeeBoardRating : Entity
{
    private DirectorSalesEmployeeBoardRating()
    {
    }

    public DirectorSalesEmployeeBoardRating(
        int employeeUserId,
        int score,
        string? note,
        int? ratedByUserId,
        string? ratedByUserName,
        DateTimeOffset ratedAtUtc)
    {
        EmployeeUserId = employeeUserId;
        Score = score;
        Note = note;
        RatedByUserId = ratedByUserId;
        RatedByUserName = ratedByUserName;
        RatedAtUtc = ratedAtUtc;
    }

    public int EmployeeUserId { get; set; }
    public int Score { get; set; }
    public string? Note { get; set; }
    public int? RatedByUserId { get; set; }
    public string? RatedByUserName { get; set; }
    public DateTimeOffset RatedAtUtc { get; set; }
}
