using Application.CustomerDecisions.Commands;
using Application.CustomerDecisions.Common;
using Application.CustomerDecisions.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/CustomerDecisions")]
public class CustomerDecisionsController : ApiController
{
    private readonly IMediator _mediator;

    public CustomerDecisionsController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.LegalNames.Read)]
    [HttpGet("legal-names")]
    public async Task<ActionResult<LegalNamesResponse>> LegalNames(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null)
        => (await _mediator.Send(new ListLegalNamesQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.CustomerDecision.Read)]
    [HttpGet]
    public async Task<ActionResult<CustomerDecisionListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? decisionType = null,
        [FromQuery] int? customerId = null,
        [FromQuery] string? search = null)
        => (await _mediator.Send(new ListCustomerDecisionsQuery
        {
            Page = page,
            PageSize = pageSize,
            DecisionType = decisionType,
            CustomerId = customerId,
            Search = search
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.CustomerDecision.Create)]
    [HttpPost]
    public async Task<ActionResult<PostBranchDecisionResultDto>> Post([FromBody] PostBody body)
        => (await _mediator.Send(new PostBranchDecisionCommand
        {
            CustomerId = body.CustomerId,
            DecisionType = body.DecisionType ?? "",
            Note = body.Note ?? "",
            OrderId = body.OrderId
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.LegalConversion.Decide)]
    [HttpPost("legal-conversions/{id:int}/approve")]
    public async Task<ActionResult<int>> ApproveConversion([FromRoute] int id, [FromBody] ConversionBody body)
        => (await _mediator.Send(new ApproveLegalConversionCommand
        {
            Id = id,
            ExpectedVersion = body.ExpectedVersion,
            Note = body.Note
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.LegalConversion.Decide)]
    [HttpPost("legal-conversions/{id:int}/reject")]
    public async Task<ActionResult<int>> RejectConversion([FromRoute] int id, [FromBody] RejectConversionBody body)
        => (await _mediator.Send(new RejectLegalConversionCommand
        {
            Id = id,
            ExpectedVersion = body.ExpectedVersion,
            Reason = body.Reason ?? ""
        }, CancellationToken)).ToHttpResponse();

    public sealed class PostBody
    {
        public int CustomerId { get; set; }
        public string? DecisionType { get; set; }
        public string? Note { get; set; }
        public int? OrderId { get; set; }
    }

    public sealed class ConversionBody
    {
        public int ExpectedVersion { get; set; }
        public string? Note { get; set; }
    }

    public sealed class RejectConversionBody
    {
        public int ExpectedVersion { get; set; }
        public string? Reason { get; set; }
    }
}
