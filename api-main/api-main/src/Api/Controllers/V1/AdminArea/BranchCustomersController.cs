using Application.BranchCustomers.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/BranchCustomers")]
public class BranchCustomersController : ApiController
{
    private readonly IMediator _mediator;

    public BranchCustomersController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("search")]
    public async Task<ActionResult<BranchCustomerSearchResponse>> Search([FromQuery] string text)
        => (await _mediator.Send(new SearchBranchCustomersQuery { Text = text ?? "" }, CancellationToken))
            .ToHttpResponse();
}
