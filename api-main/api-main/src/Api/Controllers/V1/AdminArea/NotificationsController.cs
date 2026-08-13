using Application.Notifications.Commands.Admin.MarkAllAsRead;
using Application.Notifications.Commands.Admin.MarkAsRead;
using Application.Notifications.Queries.Admin.GetById;
using Application.Notifications.Queries.Admin.GetPaginated;
using Application.Notifications.Queries.Admin.GetUnreadCount;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class NotificationsController : ApiController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Notification.Read)]
    [HttpGet("unread-count")]
    public async Task<ActionResult<GetUnreadNotificationsCountResponse>> GetUnreadCount([FromQuery] int branchId)
    {
        var query = new GetUnreadNotificationsCountQuery(branchId);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Notification.Read)]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GetPaginatedNotificationsResponse>>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedNotificationsFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedNotificationsQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Notification.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetNotificationByIdResponse>> GetById([FromRoute] int id)
    {
        var query = new GetNotificationByIdQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Notification.Update)]
    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult> MarkAsRead([FromRoute] int id)
    {
        var command = new MarkNotificationAsReadCommand(id);
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Notification.Update)]
    [HttpPatch("read-all")]
    public async Task<ActionResult<MarkAllNotificationsAsReadResponse>> MarkAllAsRead([FromQuery] int branchId)
    {
        var command = new MarkAllNotificationsAsReadCommand(branchId);
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
}
