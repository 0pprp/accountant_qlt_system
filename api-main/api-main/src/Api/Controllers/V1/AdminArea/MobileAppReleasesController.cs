using Application.MobileAppReleases.Commands;
using Application.MobileAppReleases.Common;
using Application.MobileAppReleases.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/MobileAppReleases")]
public class MobileAppReleasesController : ApiController
{
    private readonly IMediator _mediator;

    public MobileAppReleasesController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.MobileAppRelease.Read)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MobileAppReleaseDto>>> List()
        => (await _mediator.Send(new ListMobileAppReleasesQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.MobileAppRelease.Publish)]
    [HttpPut("{appKey}")]
    public async Task<ActionResult<MobileAppReleaseDto>> Publish(
        [FromRoute] string appKey,
        [FromBody] PublishBody body)
        => (await _mediator.Send(new PublishMobileAppReleaseCommand
        {
            AppKey = appKey,
            LatestVersionName = body.LatestVersionName ?? "",
            LatestVersionCode = body.LatestVersionCode,
            MinimumSupportedVersionCode = body.MinimumSupportedVersionCode,
            ForceUpdate = body.ForceUpdate,
            ApkUrl = body.ApkUrl ?? "",
            Sha256 = body.Sha256,
            ReleaseNotes = body.ReleaseNotes,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();

    public sealed class PublishBody
    {
        public string? LatestVersionName { get; set; }
        public int LatestVersionCode { get; set; }
        public int MinimumSupportedVersionCode { get; set; }
        public bool ForceUpdate { get; set; }
        public string? ApkUrl { get; set; }
        public string? Sha256 { get; set; }
        public string? ReleaseNotes { get; set; }
        public int? ExpectedVersion { get; set; }
    }
}
