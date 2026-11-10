using Application.SalesFollowup.Commands.Admin.SetAppBlock;
using Application.SalesFollowup.Commands.Admin.UpsertManagerRating;
using Application.SalesFollowup.Common;
using Application.SalesFollowup.Queries.Admin.GetAppBlock;
using Application.SalesFollowup.Queries.Admin.GetEmployeeDetail;
using Application.SalesFollowup.Queries.Admin.ListEmployees;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class SalesFollowupController : ApiController
{
    private readonly IMediator _mediator;

    public SalesFollowupController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.SalesFollowup.Read)]
    [HttpGet("employees")]
    public async Task<ActionResult<ListSalesFollowupEmployeesResponse>> ListEmployees([FromQuery] int? branchId)
    {
        var result = await _mediator.Send(new ListSalesFollowupEmployeesQuery { BranchId = branchId }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesFollowup.Read)]
    [HttpGet("employees/{employeeId:int}")]
    public async Task<ActionResult<SalesFollowupDetailDto>> GetDetail(
        [FromRoute] int employeeId,
        [FromQuery] int branchId)
    {
        var result = await _mediator.Send(
            new GetSalesFollowupEmployeeDetailQuery { EmployeeId = employeeId, BranchId = branchId },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesFollowup.Read)]
    [HttpGet("employees/{employeeId:int}/app-block")]
    public async Task<ActionResult<SalesFollowupAppBlockDto>> GetAppBlock(
        [FromRoute] int employeeId,
        [FromQuery] int branchId)
    {
        var result = await _mediator.Send(
            new GetSalesFollowupAppBlockQuery { EmployeeId = employeeId, BranchId = branchId },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesFollowup.ManageAppBlock)]
    [HttpPut("employees/{employeeId:int}/app-block")]
    public async Task<ActionResult<SalesFollowupAppBlockDto>> SetAppBlock(
        [FromRoute] int employeeId,
        [FromQuery] int branchId,
        [FromBody] SetAppBlockBody body)
    {
        var result = await _mediator.Send(
            new SetSalesFollowupAppBlockCommand
            {
                EmployeeId = employeeId,
                BranchId = branchId,
                IsBlocked = body.IsBlocked,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesFollowup.UpsertManagerRating)]
    [HttpPost("employees/{employeeId:int}/manager-rating")]
    public async Task<ActionResult<SalesFollowupManagerRatingDto>> UpsertManagerRating(
        [FromRoute] int employeeId,
        [FromQuery] int branchId,
        [FromBody] UpsertManagerRatingBody body)
    {
        var result = await _mediator.Send(
            new UpsertSalesFollowupManagerRatingCommand
            {
                EmployeeId = employeeId,
                BranchId = branchId,
                Score = body.Score,
                Note = body.Note,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    public class SetAppBlockBody
    {
        public bool IsBlocked { get; set; }
    }

    public class UpsertManagerRatingBody
    {
        public int Score { get; set; }
        public string? Note { get; set; }
    }
}
