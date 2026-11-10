using Application.Complaints.Commands;
using Application.Complaints.Common;
using Application.Complaints.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/Complaints")]
public class ComplaintsController : ApiController
{
    private readonly IMediator _mediator;

    public ComplaintsController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.Complaint.Read)]
    [HttpGet]
    public async Task<ActionResult<ComplaintListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool unreadOnly = false)
        => (await _mediator.Send(new ListComplaintsQuery
        {
            Page = page,
            PageSize = pageSize,
            UnreadOnly = unreadOnly
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Complaint.MarkRead)]
    [HttpPost("{id:int}/mark-read")]
    public async Task<ActionResult<ComplaintDto>> MarkRead([FromRoute] int id, [FromBody] VersionBody body)
        => (await _mediator.Send(new MarkComplaintReadCommand
        {
            Id = id,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();

    public sealed class VersionBody
    {
        public int ExpectedVersion { get; set; }
    }
}
