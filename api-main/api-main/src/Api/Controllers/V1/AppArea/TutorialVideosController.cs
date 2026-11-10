using Application.TutorialVideos.Common;
using Application.TutorialVideos.Queries;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/TutorialVideos")]
public class TutorialVideosController : ApiController
{
    private readonly IMediator _mediator;

    public TutorialVideosController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.Tutorial.AppRead)]
    [HttpGet("{appKey}")]
    public async Task<ActionResult<IReadOnlyList<TutorialPublicDto>>> List([FromRoute] string appKey)
        => (await _mediator.Send(new ListPublicTutorialVideosQuery { AppKey = appKey }, CancellationToken))
            .ToHttpResponse();
}
