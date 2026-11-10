using Application.MobileAppReleases.Common;
using Application.MobileAppReleases.Queries;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers.V1.AppArea;

/// <summary>Anonymous on purpose: apps check updates before sign-in.</summary>
[ApiVersion(1)]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/app/mobile-updates")]
public class MobileUpdatesController : ApiController
{
    private readonly IMediator _mediator;

    public MobileUpdatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{appKey}")]
    public async Task<ActionResult<MobileUpdateCheckDto>> Get(
        [FromRoute] string appKey,
        [FromQuery] int? currentVersionCode)
        => (await _mediator.Send(new EvaluateMobileUpdateQuery
        {
            AppKey = appKey,
            CurrentVersionCode = currentVersionCode ?? 0
        }, CancellationToken)).ToHttpResponse();
}
