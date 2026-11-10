using Application.Announcements.Common;
using Application.Announcements.Queries;
using Microsoft.AspNetCore.Hosting;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/Announcements")]
public class AnnouncementsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public AnnouncementsController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    [HasPermission(Permissions.Announcement.AppRead)]
    [HttpGet("{appKey}")]
    public async Task<ActionResult<IReadOnlyList<AnnouncementPublicDto>>> List([FromRoute] string appKey)
        => (await _mediator.Send(new ListPublicAnnouncementsQuery { AppKey = appKey }, CancellationToken))
            .ToHttpResponse();

    /// <summary>حراسة داخل المعالج: AppRead أو Read.</summary>
    [HttpGet("media/{fileName}")]
    public async Task<IActionResult> Media([FromRoute] string fileName)
    {
        var result = await _mediator.Send(new GetAnnouncementMediaQuery { FileName = fileName }, CancellationToken);
        if (result.IsFailed)
            return result.ToHttpResponse().Result!;

        var dto = result.Data!;
        var root = _env.WebRootPath ?? _env.ContentRootPath;
        var full = Path.IsPathRooted(dto.RelativePath)
            ? dto.RelativePath
            : Path.Combine(root, dto.RelativePath);
        if (System.IO.File.Exists(full) == false)
            return NotFound();

        return PhysicalFile(full, dto.ContentType, enableRangeProcessing: true);
    }
}
