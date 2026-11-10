using Application.TutorialVideos.Commands;
using Application.TutorialVideos.Common;
using Application.TutorialVideos.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/TutorialVideos")]
public class TutorialVideosController : ApiController
{
    private readonly IMediator _mediator;

    public TutorialVideosController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.Tutorial.Read)]
    [HttpGet]
    public async Task<ActionResult<TutorialVideoListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? appKey = null,
        [FromQuery] bool? isEnabled = null)
        => (await _mediator.Send(new ListTutorialVideosQuery
        {
            Page = page,
            PageSize = pageSize,
            AppKey = appKey,
            IsEnabled = isEnabled
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Tutorial.Read)]
    [HttpGet("apps")]
    public async Task<ActionResult<IReadOnlyList<TutorialAppOptionDto>>> Apps()
        => (await _mediator.Send(new ListTutorialAppsQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Tutorial.Create)]
    [HttpPost]
    public async Task<ActionResult<TutorialVideoDto>> Create([FromBody] UpsertBody body)
        => (await _mediator.Send(new CreateTutorialVideoCommand
        {
            AppKey = body.AppKey ?? "",
            QuestionAr = body.QuestionAr ?? "",
            VideoUrl = body.VideoUrl ?? "",
            IsEnabled = body.IsEnabled
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Tutorial.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TutorialVideoDto>> Update([FromRoute] int id, [FromBody] UpdateBody body)
        => (await _mediator.Send(new UpdateTutorialVideoCommand
        {
            Id = id,
            QuestionAr = body.QuestionAr ?? "",
            VideoUrl = body.VideoUrl ?? "",
            IsEnabled = body.IsEnabled,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Tutorial.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id, [FromQuery] int expectedVersion)
        => (await _mediator.Send(new DeleteTutorialVideoCommand
        {
            Id = id,
            ExpectedVersion = expectedVersion
        }, CancellationToken)).ToHttpResponse();

    public sealed class UpsertBody
    {
        public string? AppKey { get; set; }
        public string? QuestionAr { get; set; }
        public string? VideoUrl { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public sealed class UpdateBody
    {
        public string? QuestionAr { get; set; }
        public string? VideoUrl { get; set; }
        public bool IsEnabled { get; set; }
        public int ExpectedVersion { get; set; }
    }
}
