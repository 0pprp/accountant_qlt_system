namespace Application.FollowerWork.Commands.Admin.UpdateFollower;

public record UpdateFollowerCommand : IRequest<Result>
{
    public int FollowerId { get; set; }
    public required string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
    public List<int> AssignListIds { get; set; } = [];
}
