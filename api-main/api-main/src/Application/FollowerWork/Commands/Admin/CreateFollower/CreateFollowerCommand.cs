namespace Application.FollowerWork.Commands.Admin.CreateFollower;

public record CreateFollowerCommand : IRequest<Result<int>>
{
    public required string FullName { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? PhoneNumber { get; set; }
    public required List<int> BranchIds { get; set; }
    public List<int> ListIds { get; set; } = [];
}
