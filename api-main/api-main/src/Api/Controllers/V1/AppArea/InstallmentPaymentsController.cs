using Application.Common.Models.KeysetPagination;
using Application.Common.Utilities;
using Application.InstallmentPayments.Commands.App.Sync;
using Application.InstallmentPayments.Queries.App.GetDailyPdfReport;
using Application.InstallmentPayments.Queries.App.GetPaginated;
using Application.InstallmentPayments.Queries.App.GetSummary;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class InstallmentPaymentsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public InstallmentPaymentsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetPaginatedInstallmentPaymentsResponse>>> GetPaginated(
        [FromQuery] KeysetPagination<DateTimeOffset?> keysetPagination, [FromQuery] GetPaginatedInstallmentPaymentsFilter filter)
    {
        var query = new GetPaginatedInstallmentPaymentsQuery
        {
            Pagination = keysetPagination,
            Filter = filter,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet("summary")]
    public async Task<ActionResult<GetInstallmentPaymentsSummaryResponse>> GetSummary(
        [FromQuery] GetPaginatedInstallmentPaymentsFilter filter)
    {
        var query = new GetInstallmentPaymentsSummaryQuery
        {
            Filter = filter,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet("daily-pdf-report")]
    public async Task<ActionResult> GetDailyPdfReport([FromQuery] GetDailyInstallmentPaymentsPdfReportFilter filter,
        [FromQuery] PdfSettings? pdfSettings)
    {
        var query = new GetDailyInstallmentPaymentsPdfReportQuery
        {
            UserId = _currentUserService.UserId!.Value,
            PdfSettings = pdfSettings,
            ShowPaidInstallments = filter.ShowPaidInstallments
        };
        var result = await _mediator.Send(query, CancellationToken);

        var pdfFile = result.Data;

        return File(pdfFile!.Data, pdfFile.ContentType, pdfFile.FileName);
    }

    [HasPermission(Permissions.InstallmentPayment.Create)]
    [HttpPost("sync")]
    public async Task<ActionResult> Sync([FromBody] SyncInstallmentPaymentsDto dto)
    {
        var command = new SyncInstallmentPaymentsCommand
        {
            InstallmentPayments = dto.InstallmentPayments,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
}