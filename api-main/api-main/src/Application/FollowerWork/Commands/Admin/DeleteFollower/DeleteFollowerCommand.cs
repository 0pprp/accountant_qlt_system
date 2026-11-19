namespace Application.FollowerWork.Commands.Admin.DeleteFollower;

public record DeleteFollowerCommand(int FollowerId) : IRequest<Result>;
