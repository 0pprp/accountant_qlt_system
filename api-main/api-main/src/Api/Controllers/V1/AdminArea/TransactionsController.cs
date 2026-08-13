using Application.Transactions.Commands.Admin.ChangeStatus;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class TransactionsController : ApiController
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HasPermission(Permissions.Transaction.Update)]
    [HttpPut("{id:int}/change-status")]
    public async Task<ActionResult> ChangeStatus([FromRoute] int id, [FromBody] ChangeTransactionStatusDto dto)
    {
        var command = new ChangeTransactionStatusCommand
        {
            Id = id,
            Status = dto.Status,
            StatusDescription = dto.StatusDescription
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
}