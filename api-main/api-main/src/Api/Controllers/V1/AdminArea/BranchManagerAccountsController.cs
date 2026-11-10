using Application.BranchManagerAccounts.Commands;
using Application.BranchManagerAccounts.Common;
using Application.BranchManagerAccounts.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/BranchManagerAccounts")]
public class BranchManagerAccountsController : ApiController
{
    private readonly IMediator _mediator;

    public BranchManagerAccountsController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.BranchManagerAccount.Read)]
    [HttpGet]
    public async Task<ActionResult<BranchManagerAccountListResponse>> List()
        => (await _mediator.Send(new ListBranchManagerAccountsQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.BranchManagerAccount.Read)]
    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<BranchOptionDto>>> Cities()
        => (await _mediator.Send(new ListBranchManagerAccountCitiesQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.BranchManagerAccount.Write)]
    [HttpPost]
    public async Task<ActionResult<BranchManagerAccountDto>> Create([FromBody] CreateBody body)
        => (await _mediator.Send(new CreateBranchManagerAccountCommand
        {
            Username = body.Username ?? "",
            Password = body.Password ?? "",
            FullName = body.FullName ?? "",
            BranchIds = body.BranchIds ?? []
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.BranchManagerAccount.Write)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BranchManagerAccountDto>> Update([FromRoute] int id, [FromBody] UpdateBody body)
        => (await _mediator.Send(new UpdateBranchManagerAccountCommand
        {
            Id = id,
            Username = body.Username ?? "",
            Password = body.Password,
            FullName = body.FullName ?? "",
            IsActive = body.IsActive,
            BranchIds = body.BranchIds ?? []
        }, CancellationToken)).ToHttpResponse();

    public sealed class CreateBody
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public List<int>? BranchIds { get; set; }
    }

    public sealed class UpdateBody
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public List<int>? BranchIds { get; set; }
    }
}
