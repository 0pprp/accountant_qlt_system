using Application.CommitteeTasks.Commands.Admin.Upsert;
using Application.CommitteeTasks.Common;
using Application.CommitteeTasks.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/CommitteeTasks")]
public class CommitteeTasksController : ApiController
{
    private readonly IMediator _mediator;

    public CommitteeTasksController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.CommitteeTask.Read)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommitteeTaskRowDto>>> List()
        => (await _mediator.Send(new ListAdminCommitteeTasksQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.CommitteeTask.Write)]
    [HttpPost]
    public async Task<ActionResult<CommitteeTaskRowDto>> Create([FromBody] UpsertBody? body)
    {
        body ??= new UpsertBody();
        return (await _mediator.Send(new CreateCommitteeTaskCommand
        {
            Body = body.Body ?? "",
            IsEnabled = body.IsEnabled
        }, CancellationToken)).ToHttpResponse();
    }

    [HasPermission(Permissions.CommitteeTask.Write)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommitteeTaskRowDto>> Update([FromRoute] int id, [FromBody] UpsertBody? body)
    {
        body ??= new UpsertBody();
        return (await _mediator.Send(new UpdateCommitteeTaskCommand
        {
            Id = id,
            Body = body.Body ?? "",
            IsEnabled = body.IsEnabled,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();
    }

    [HasPermission(Permissions.CommitteeTask.Write)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id, [FromQuery] int expectedVersion)
        => (await _mediator.Send(new DeleteCommitteeTaskCommand
        {
            Id = id,
            ExpectedVersion = expectedVersion
        }, CancellationToken)).ToHttpResponse();

    public sealed class UpsertBody
    {
        public string? Body { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int ExpectedVersion { get; set; }
    }
}
