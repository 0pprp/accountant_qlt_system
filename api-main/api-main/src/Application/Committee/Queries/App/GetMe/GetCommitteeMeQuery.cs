namespace Application.Committee.Queries.App.GetMe;

public record GetCommitteeMeQuery(int UserId, string? Role) : IRequest<Result<GetCommitteeMeResponse>>;

public record GetCommitteeMeResponse
{
    public int UserId { get; set; }
    public required string FullName { get; set; }
    public required string Role { get; set; }
    public required string ProductRoleLabel { get; set; }
    public bool IsCommitteeOfficer { get; set; }
    public List<int> BranchIds { get; set; } = [];
}
