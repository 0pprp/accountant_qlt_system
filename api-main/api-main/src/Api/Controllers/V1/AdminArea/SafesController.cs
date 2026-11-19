using Application.Common.Models;
using Application.Common.SeedData;
using Application.Safes.Commands.Admin.BackfillPaymentCollections;
using Application.Safes.Commands.Admin.Create;
using Application.Safes.Commands.Admin.Delete;
using Application.Safes.Commands.Admin.Deposit;
using Application.Safes.Commands.Admin.Transfer;
using Application.Safes.Commands.Admin.Withdraw;
using Application.Safes.Queries.Admin.GetAll;
using Application.Safes.Queries.Admin.GetMovements;
using Application.Safes.Queries.Admin.GetMovementsExcelReport;
using Application.Safes.Queries.Admin.Verify;
using Api.Authorization;
using Api.Extensions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class SafesController : ApiController
{
    private readonly ISender _mediator;

    public SafesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Safe.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetAllSafesResponse>>> GetAll([FromQuery] int branchId)
    {
        var result = await _mediator.Send(new GetAllSafesQuery(branchId), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Safe.Read)]
    [HttpGet("{id:int}/movements")]
    public async Task<ActionResult<PaginatedList<GetSafeMovementsResponse>>> GetMovements(
        [FromRoute] int id,
        [FromQuery] Pagination pagination,
        [FromQuery] GetSafeMovementsFilter filter)
    {
        var result = await _mediator.Send(
            new GetSafeMovementsQuery { SafeId = id, Pagination = pagination, Filter = filter },
            CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Safe.Read)]
    [HttpGet("{id:int}/movements/excel-report")]
    public async Task<ActionResult> GetMovementsExcelReport(
        [FromRoute] int id, [FromQuery] GetSafeMovementsFilter filter)
    {
        var result = await _mediator.Send(
            new GetSafeMovementsExcelReportQuery { SafeId = id, Filter = filter }, CancellationToken);

        if (result.IsFailed)
            return BadRequest(result.Error);

        return File(result.Data!.Data, result.Data.ContentType, result.Data.FileName);
    }

    /// <summary>تحقق حسابي: الرصيد المخزون مقابل مجموع حركات السجل.</summary>
    [HasPermission(Permissions.Safe.Read)]
    [HttpGet("{id:int}/verify")]
    public async Task<ActionResult<VerifySafeBalanceResponse>> Verify([FromRoute] int id)
    {
        var result = await _mediator.Send(new VerifySafeBalanceQuery(id), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Safe.Create)]
    [HttpPost]
    public async Task<ActionResult<CreateSafeResponse>> Create([FromBody] CreateSafeCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Safe.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteSafeCommand(id), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Transaction.Create)]
    [HttpPost("{id:int}/deposit")]
    public async Task<ActionResult> Deposit([FromRoute] int id, [FromBody] SafeAmountDto dto)
    {
        var result = await _mediator.Send(
            new DepositToSafeCommand
            {
                SafeId = id,
                Amount = dto.Amount,
                Note = dto.Note,
                OperationKey = dto.OperationKey
            },
            CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Transaction.Create)]
    [HttpPost("{id:int}/withdraw")]
    public async Task<ActionResult> Withdraw([FromRoute] int id, [FromBody] SafeAmountDto dto)
    {
        var result = await _mediator.Send(
            new WithdrawFromSafeCommand
            {
                SafeId = id,
                Amount = dto.Amount,
                Note = dto.Note,
                OperationKey = dto.OperationKey
            },
            CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Safe.Create)]
    [HttpPost("backfill-payment-collections")]
    public async Task<ActionResult<BackfillPaymentCollectionsResponse>> BackfillPaymentCollections(
        [FromQuery] int branchId, [FromQuery] bool dryRun = true)
    {
        var result = await _mediator.Send(
            new BackfillPaymentCollectionsCommand { BranchId = branchId, DryRun = dryRun }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Transaction.Create)]
    [HttpPost("transfer")]
    public async Task<ActionResult> Transfer([FromBody] TransferBetweenSafesCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);
        return result.ToHttpResponse();
    }
}

public record SafeAmountDto
{
    public double Amount { get; set; }
    public string? Note { get; set; }
    public Guid? OperationKey { get; set; }
}
