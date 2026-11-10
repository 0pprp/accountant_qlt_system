using Application.SalesEmployees.Commands.Admin.EndAssignment;
using Application.SalesEmployees.Commands.Admin.GrantAssignment;
using Application.SalesEmployees.Common;
using Application.SalesEmployees.Queries.Admin.GetRatingSummary;
using Application.SalesEmployees.Queries.Admin.List;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class SalesEmployeesController : ApiController
{
    private readonly IMediator _mediator;

    public SalesEmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.SalesEmployee.Read)]
    [HttpGet]
    public async Task<ActionResult<ListSalesEmployeesResponse>> List(
        [FromQuery] int? branchId,
        [FromQuery] string? search,
        [FromQuery] string? shiftState,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null)
    {
        var result = await _mediator.Send(
            new ListSalesEmployeesQuery
            {
                BranchId = branchId,
                Search = search,
                ShiftState = shiftState,
                Page = page,
                PageSize = pageSize,
                Sort = sort,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesEmployee.Read)]
    [HttpGet("{employeeId:int}/rating-summary")]
    public async Task<ActionResult<SalesEmployeeRatingSummaryDto>> RatingSummary(
        [FromRoute] int employeeId,
        [FromQuery] int? branchId,
        [FromQuery] string? bucket)
    {
        var result = await _mediator.Send(
            new GetSalesEmployeeRatingSummaryQuery
            {
                EmployeeId = employeeId,
                BranchId = branchId,
                Bucket = bucket,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesEmployee.ManageAssignment)]
    [HttpPost("assignments")]
    public async Task<ActionResult<GrantSalesEmployeeAssignmentResponse>> GrantAssignment(
        [FromBody] GrantAssignmentBody body)
    {
        var result = await _mediator.Send(
            new GrantSalesEmployeeAssignmentCommand
            {
                UserId = body.UserId,
                BranchId = body.BranchId,
                Note = body.Note,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesEmployee.ManageAssignment)]
    [HttpPost("assignments/{assignmentId:int}/end")]
    public async Task<ActionResult> EndAssignment([FromRoute] int assignmentId)
    {
        var result = await _mediator.Send(
            new EndSalesEmployeeAssignmentCommand { AssignmentId = assignmentId },
            CancellationToken);
        return result.ToHttpResponse();
    }
}

public sealed class GrantAssignmentBody
{
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public string? Note { get; set; }
}

