using Application.Users.Queries.App.GetProfile;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.User.Read)]
    [HttpGet("profile")]
    public async Task<ActionResult<GetUserProfileResponse>> GetProfile()
    {
        var query = new GetUserProfileQuery(_currentUserService.UserId!.Value);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}