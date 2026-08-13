namespace Application.Users.Queries.Admin.GetProfile;

public record GetUserProfileQuery(int Id) : IRequest<Result<GetUserProfileResponse>>;