using Application.InstallmentPayments.Commands.Admin.Create;
using Application.InstallmentPayments.Commands.Admin.Delete;
using Application.InstallmentPayments.Commands.Admin.Update;
using Application.InstallmentPayments.Queries.Admin.GetExcelReport;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class InstallmentPaymentsController : ApiController
{
    private readonly IMediator _mediator;

    public InstallmentPaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.InstallmentPayment.Create)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateInstallmentPaymentCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.InstallmentPayment.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateInstallmentPaymentDto dto)
    {
        var command = new UpdateInstallmentPaymentCommand
        {
            Id = id,
            Amount = dto.Amount,
            Description = dto.Description
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet("excel-report")]
    public async Task<ActionResult> GetExcelReport([FromQuery] GetInstallmentPaymentsExcelReportQuery query)
    {
        var result = await _mediator.Send(query, CancellationToken);

        var excelFile = result.Data;

        return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
    }

    [HasPermission(Permissions.InstallmentPayment.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        var command = new DeleteInstallmentPaymentCommand(id);
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
}