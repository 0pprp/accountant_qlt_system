using Application.Announcements.Commands;
using Application.Announcements.Common;
using Application.Announcements.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/Announcements")]
public class AnnouncementsController : ApiController
{
    private readonly IMediator _mediator;

    public AnnouncementsController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.Announcement.Read)]
    [HttpGet]
    public async Task<ActionResult<AnnouncementListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? appKey = null,
        [FromQuery] bool? isEnabled = null)
        => (await _mediator.Send(new ListAnnouncementsQuery
        {
            Page = page,
            PageSize = pageSize,
            AppKey = appKey,
            IsEnabled = isEnabled
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Announcement.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AnnouncementAdminDto>> Get([FromRoute] int id)
        => (await _mediator.Send(new GetAnnouncementByIdQuery { Id = id }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Announcement.Create)]
    [HttpPost]
    public async Task<ActionResult<AnnouncementAdminDto>> Create([FromBody] UpsertBody body)
        => (await _mediator.Send(new CreateAnnouncementCommand
        {
            AppKey = body.AppKey ?? "",
            ContentType = body.ContentType ?? "text",
            TextBody = body.TextBody,
            BannerStorageKey = body.BannerStorageKey,
            ZoomStorageKey = body.ZoomStorageKey,
            HasLink = body.HasLink,
            LinkUrl = body.LinkUrl,
            IsEnabled = body.IsEnabled
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Announcement.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AnnouncementAdminDto>> Update([FromRoute] int id, [FromBody] UpdateBody body)
        => (await _mediator.Send(new UpdateAnnouncementCommand
        {
            Id = id,
            ContentType = body.ContentType ?? "text",
            TextBody = body.TextBody,
            BannerStorageKey = body.BannerStorageKey,
            ZoomStorageKey = body.ZoomStorageKey,
            HasLink = body.HasLink,
            LinkUrl = body.LinkUrl,
            IsEnabled = body.IsEnabled,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Announcement.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id, [FromQuery] int expectedVersion)
        => (await _mediator.Send(new DeleteAnnouncementCommand
        {
            Id = id,
            ExpectedVersion = expectedVersion
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Announcement.Create)]
    [HttpPost("upload-image")]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<ActionResult<AnnouncementUploadResult>> Upload([FromForm] IFormFile file)
        => (await _mediator.Send(new UploadAnnouncementImageCommand { File = file }, CancellationToken))
            .ToHttpResponse();

    public sealed class UpsertBody
    {
        public string? AppKey { get; set; }
        public string? ContentType { get; set; }
        public string? TextBody { get; set; }
        public string? BannerStorageKey { get; set; }
        public string? ZoomStorageKey { get; set; }
        public bool HasLink { get; set; }
        public string? LinkUrl { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public sealed class UpdateBody
    {
        public string? ContentType { get; set; }
        public string? TextBody { get; set; }
        public string? BannerStorageKey { get; set; }
        public string? ZoomStorageKey { get; set; }
        public bool HasLink { get; set; }
        public string? LinkUrl { get; set; }
        public bool IsEnabled { get; set; }
        public int ExpectedVersion { get; set; }
    }
}
