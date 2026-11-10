using Application.SalesRequests.Queries.Admin.FilterOptions;
using Application.SalesRequests.Commands.Admin.ApproveException;
using Application.SalesRequests.Commands.Admin.Assign;
using Application.SalesRequests.Commands.Admin.Reject;
using Application.SalesRequests.Commands.Admin.RejectException;
using Application.SalesRequests.Commands.Admin.RequestException;
using Application.SalesRequests.Commands.Admin.Restore;
using Application.SalesRequests.Commands.Admin.SoftDelete;
using Application.SalesRequests.Queries.Admin.AssignableEmployees;
using Application.SalesRequests.Queries.Admin.Counts;
using Application.SalesRequests.Queries.Admin.GetById;
using Application.SalesRequests.Queries.Admin.GetDuplicateRequests;
using Application.SalesRequests.Queries.Admin.GetMatchHits;
using Application.SalesRequests.Queries.Admin.GetMatchSummaryBatch;
using Application.SalesRequests.Queries.Admin.GetUnassigned;
using Application.SalesRequests.Queries.Admin.List;
using Application.SalesRequests.Queries.Admin.Timeline;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class SalesRequestsController : ApiController
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("filter-options")]
    public async Task<ActionResult<SalesRequestFilterOptions>> FilterOptions([FromQuery] string tab = "unassigned", [FromQuery] int? branchId = null)
        => (await _mediator.Send(new GetSalesRequestFilterOptionsQuery(tab, branchId), CancellationToken)).ToHttpResponse();

    private readonly IMediator _mediator;

    public SalesRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// قائمة طلبات البيع حسب التبويب — الصلاحية تُفحص في المعالج لكل تبويب.
    /// «الكل» = اتحاد السجلات المسموح بها فقط.
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<AdminSalesRequestListItem>>> List(
        [FromQuery] Pagination pagination,
        [FromQuery] string? tab,
        [FromQuery] int? employeeId,
        [FromQuery] int? branchId,
        [FromQuery] int? provinceId,
        [FromQuery] SalesRequestSourceType? sourceType,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? search,
        [FromQuery] SalesRequestExceptionHoldStatus? exceptionStatus)
    {
        var result = await _mediator.Send(new GetAdminSalesRequestsQuery
        {
            Pagination = pagination,
            Tab = tab ?? "unassigned",
            EmployeeId = employeeId,
            BranchId = branchId,
            ProvinceId = provinceId,
            SourceType = sourceType,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            ExceptionStatus = exceptionStatus
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [Authorize]
    [HttpGet("counts")]
    public async Task<ActionResult<AdminSalesRequestCountsResponse>> Counts(
        [FromQuery] int? employeeId,
        [FromQuery] int? branchId,
        [FromQuery] int? provinceId,
        [FromQuery] SalesRequestSourceType? sourceType,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? search)
    {
        var result = await _mediator.Send(new GetAdminSalesRequestCountsQuery
        {
            EmployeeId = employeeId,
            BranchId = branchId,
            ProvinceId = provinceId,
            SourceType = sourceType,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.Assign)]
    [HttpGet("assignable-employees")]
    public async Task<ActionResult<List<AssignableEmployeeItem>>> AssignableEmployees(
        [FromQuery] int branchId,
        [FromQuery] string? search)
    {
        var result = await _mediator.Send(
            new GetAssignableEmployeesQuery(branchId, search), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.ReadUnassigned)]
    [HttpGet("unassigned")]
    public async Task<ActionResult<PaginatedList<GetUnassignedSalesRequestItem>>> GetUnassigned(
        [FromQuery] Pagination pagination,
        [FromQuery] int? branchId,
        [FromQuery] int? provinceId,
        [FromQuery] SalesRequestSourceType? sourceType,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? search)
    {
        var result = await _mediator.Send(new GetUnassignedSalesRequestsQuery
        {
            Pagination = pagination,
            Filter = new GetUnassignedSalesRequestsFilter
            {
                BranchId = branchId,
                ProvinceId = provinceId,
                SourceType = sourceType,
                FromDate = fromDate,
                ToDate = toDate,
                Search = search
            }
        }, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.MatchRead)]
    [HttpPost("match-summary")]
    public async Task<ActionResult<GetSalesRequestMatchSummaryBatchResponse>> MatchSummary(
        [FromBody] MatchSummaryBody body)
    {
        var result = await _mediator.Send(
            new GetSalesRequestMatchSummaryBatchQuery(body.RequestIds ?? []),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminSalesRequestListItem>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetAdminSalesRequestByIdQuery(id), CancellationToken);
        return result.ToHttpResponse();
    }

    [Authorize]
    [HttpGet("{id:int}/timeline")]
    public async Task<ActionResult<List<AdminSalesRequestTimelineItem>>> Timeline([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetAdminSalesRequestTimelineQuery(id), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.MatchRead)]
    [HttpGet("{id:int}/match-hits")]
    public async Task<ActionResult<PaginatedList<GetSalesRequestMatchHitItem>>> MatchHits(
        [FromRoute] int id,
        [FromQuery] string criterion,
        [FromQuery] Pagination pagination)
    {
        var result = await _mediator.Send(new GetSalesRequestMatchHitsQuery
        {
            RequestId = id,
            Criterion = criterion,
            Pagination = pagination
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.MatchRead)]
    [HttpGet("{id:int}/duplicate-requests")]
    public async Task<ActionResult<PaginatedList<GetDuplicateSalesRequestItem>>> DuplicateRequests(
        [FromRoute] int id,
        [FromQuery] Pagination pagination)
    {
        var result = await _mediator.Send(new GetDuplicateSalesRequestsQuery
        {
            RequestId = id,
            Pagination = pagination
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.Assign)]
    [HttpPost("{id:int}/assign")]
    public async Task<ActionResult> Assign([FromRoute] int id, [FromBody] AssignBody body)
    {
        var result = await _mediator.Send(new AssignSalesRequestCommand
        {
            RequestId = id,
            EmployeeId = body.EmployeeId,
            Reason = body.Reason
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.Reject)]
    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult> Reject([FromRoute] int id, [FromBody] RejectBody body)
    {
        var result = await _mediator.Send(new RejectSalesRequestCommand
        {
            RequestId = id,
            Reason = body.Reason
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.RequestException)]
    [HttpPost("{id:int}/request-exception")]
    public async Task<ActionResult> RequestException([FromRoute] int id, [FromBody] ExceptionBody body)
    {
        var result = await _mediator.Send(new RequestSalesRequestExceptionCommand
        {
            RequestId = id,
            Reason = body.Reason
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.ExceptionDecide)]
    [HttpPost("{id:int}/approve-exception")]
    public async Task<ActionResult> ApproveException([FromRoute] int id, [FromBody] DecisionBody body)
    {
        var result = await _mediator.Send(new ApproveSalesRequestExceptionCommand
        {
            RequestId = id,
            Reason = body.Reason ?? string.Empty
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.ExceptionDecide)]
    [HttpPost("{id:int}/reject-exception")]
    public async Task<ActionResult> RejectException([FromRoute] int id, [FromBody] DecisionBody body)
    {
        var result = await _mediator.Send(new RejectSalesRequestExceptionCommand
        {
            RequestId = id,
            Reason = body.Reason ?? string.Empty
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.SoftDelete)]
    [HttpPost("{id:int}/soft-delete")]
    public async Task<ActionResult> SoftDelete([FromRoute] int id, [FromBody] DecisionBody body)
    {
        var result = await _mediator.Send(new SoftDeleteSalesRequestCommand
        {
            RequestId = id,
            Reason = body.Reason ?? string.Empty
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.Restore)]
    [HttpPost("{id:int}/restore")]
    public async Task<ActionResult> Restore([FromRoute] int id, [FromBody] RestoreBody body)
    {
        var result = await _mediator.Send(new RestoreSalesRequestCommand
        {
            RequestId = id,
            Reason = body.Reason
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    public record MatchSummaryBody(List<int>? RequestIds);
    public record AssignBody(int EmployeeId, string? Reason);
    public record RejectBody(string? Reason);
    public record ExceptionBody(string? Reason);
    public record DecisionBody(string? Reason);
    public record RestoreBody(string? Reason);
}
