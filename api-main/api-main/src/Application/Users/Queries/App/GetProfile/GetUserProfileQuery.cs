namespace Application.Users.Queries.App.GetProfile;

public record GetUserProfileQuery(int Id) : IRequest<Result<GetUserProfileResponse>>;