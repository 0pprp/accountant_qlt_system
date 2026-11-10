using Application.DelegateFollowup.Commands.Admin.UpsertBoardOperatorRating;
using Application.DelegateFollowup.Common;
using Application.DelegateFollowup.Queries.Admin.GetBoardRating;
using Application.DelegateFollowup.Queries.Admin.GetCustomerPayments;
using Application.DelegateFollowup.Queries.Admin.GetCustomers;
using Application.DelegateFollowup.Queries.Admin.GetLists;
using Application.DelegateFollowup.Queries.Admin.GetSummary;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class DelegateFollowupController : ApiController
{
    private readonly IMediator _mediator;

    public DelegateFollowupController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.DelegateFollowup.Read)]
    [HttpGet("lists")]
    public async Task<ActionResult<GetDelegateFollowupListsResponse>> GetLists([FromQuery] int? branchId)
    {
        var result = await _mediator.Send(new GetDelegateFollowupListsQuery { BranchId = branchId }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.DelegateFollowup.Read)]
    [HttpGet("lists/{listId:int}/summary")]
    public async Task<ActionResult<DelegateFollowupSummaryDto>> GetSummary(
        [FromRoute] int listId,
        [FromQuery] int branchId)
    {
        var result = await _mediator.Send(
            new GetDelegateFollowupSummaryQuery { ListId = listId, BranchId = branchId },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.DelegateFollowup.Read)]
    [HttpGet("lists/{listId:int}/customers")]
    public async Task<ActionResult<DelegateFollowupCustomersDto>> GetCustomers(
        [FromRoute] int listId,
        [FromQuery] int branchId)
    {
        var result = await _mediator.Send(
            new GetDelegateFollowupCustomersQuery { ListId = listId, BranchId = branchId },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.DelegateFollowup.Read)]
    [HttpGet("lists/{listId:int}/board-operator-rating")]
    public async Task<ActionResult<DelegateFollowupBoardRatingDto>> GetBoardOperatorRating(
        [FromRoute] int listId,
        [FromQuery] int branchId,
        [FromQuery] DateOnly? ratingDate)
    {
        var result = await _mediator.Send(
            new GetDelegateFollowupBoardRatingQuery
            {
                ListId = listId,
                BranchId = branchId,
                RatingDate = ratingDate
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>تسديدات زبون ضمن القائمة — قراءة فقط (مرجع الشاشات لا ينشئ تسديدًا من هنا).</summary>
    [HasPermission(Permissions.DelegateFollowup.Read)]
    [HttpGet("lists/{listId:int}/customers/{customerId:int}/payments")]
    public async Task<ActionResult<DelegateFollowupCustomerPaymentsDto>> GetCustomerPayments(
        [FromRoute] int listId,
        [FromRoute] int customerId,
        [FromQuery] int branchId)
    {
        var result = await _mediator.Send(
            new GetDelegateFollowupCustomerPaymentsQuery
            {
                ListId = listId,
                BranchId = branchId,
                CustomerId = customerId
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.DelegateFollowup.UpsertBoardRating)]
    [HttpPost("lists/{listId:int}/board-operator-rating")]
    public async Task<ActionResult<DelegateFollowupActionResultDto>> UpsertBoardOperatorRating(
        [FromRoute] int listId,
        [FromQuery] int branchId,
        [FromBody] UpsertBoardOperatorRatingBody body)
    {
        var result = await _mediator.Send(
            new UpsertBoardOperatorRatingCommand
            {
                ListId = listId,
                BranchId = branchId,
                Score = body.Score,
                Note = body.Note
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    public class UpsertBoardOperatorRatingBody
    {
        public int Score { get; set; }
        public string? Note { get; set; }
    }
}
