using Application.MobileAppReleases.Common;
using Application.MobileAppReleases.Queries;
using Domain.Entities.MobileAppReleaseAggregate;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers.V1.AppArea;

/// <summary>
/// توافق قديم: checkVersion → مخزن MobileAppReleases.
/// المسار الجديد المفضل: GET /app/mobile-updates/{appKey}.
/// </summary>
[ApiVersion(1)]
[AllowAnonymous]
public class AppController : ApiController
{
    private readonly IMediator _mediator;

    public AppController(IMediator mediator) => _mediator = mediator;

    [HttpGet("checkVersion")]
    public async Task<ActionResult<LegacyAppVersionResponse>> CheckVersion(
        [FromQuery] int buildNumber,
        [FromQuery] string? appKey = null)
    {
        var key = string.IsNullOrWhiteSpace(appKey)
            ? MobileAppKeys.SalesEmployee
            : appKey;
        var result = await _mediator.Send(new EvaluateMobileUpdateQuery
        {
            AppKey = key,
            CurrentVersionCode = buildNumber
        }, CancellationToken);

        if (result.IsFailed)
            return Ok(new LegacyAppVersionResponse(false, false, ""));

        return Ok(MobileAppReleaseMapper.ToLegacy(result.Data!));
    }
}
