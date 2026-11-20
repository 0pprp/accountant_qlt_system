using Api.Authorization;
using Api.Extensions;
using Application.Common.SeedData;
using Application.FieldPromoters.Admin;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AdminArea;

/// <summary>متابعة المروّجين الميدانيين.</summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/field-promoters")]
public class FieldPromotersController(IMediator mediator) : ApiController
{
    private readonly IMediator _mediator = mediator;

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("live")]
    public async Task<ActionResult<PromotersLiveDto>> Live([FromQuery] int? branchId)
        => (await _mediator.Send(new GetPromotersLiveQuery(branchId), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("{userId:int}/day")]
    public async Task<ActionResult<PromoterDayDto>> Day(int userId, [FromQuery] DateOnly? date)
        => (await _mediator.Send(new GetPromoterDayQuery(userId, date), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("attendance")]
    public async Task<ActionResult<AttendanceDto>> Attendance([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int? branchId)
        => (await _mediator.Send(new GetPromoterAttendanceQuery(from, to, branchId), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("attendance/export")]
    public async Task<IActionResult> ExportAttendance([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int? branchId)
    {
        var result = await _mediator.Send(new ExportPromoterAttendanceQuery(from, to, branchId), CancellationToken);
        if (result.IsFailed) return result.ToHttpResponse().Result!;
        return File(result.Data!.Data, result.Data.ContentType, result.Data.FileName);
    }

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("alerts")]
    public async Task<ActionResult<PromoterAlertsPage>> Alerts([FromQuery] bool openOnly = false, [FromQuery] int page = 1)
        => (await _mediator.Send(new GetPromoterAlertsQuery(openOnly, page), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpPost("alerts/{id:int}/seen")]
    public async Task<ActionResult> AlertSeen(int id)
        => (await _mediator.Send(new MarkPromoterAlertSeenCommand(id), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpPost("alerts/seen-all")]
    public async Task<ActionResult> AlertsSeenAll()
        => (await _mediator.Send(new MarkPromoterAlertSeenCommand(null), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("pins")]
    public async Task<ActionResult<AdminPinsPage>> Pins([FromQuery] int? branchId, [FromQuery] int? userId,
        [FromQuery] byte? minInterest, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? q,
        [FromQuery] int page = 1)
        => (await _mediator.Send(new GetAdminPinsQuery(branchId, userId, minInterest, from, to, q, page), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Manage)]
    [HttpDelete("pins/{id:int}")]
    public async Task<ActionResult> DeletePin(int id)
        => (await _mediator.Send(new DeleteAdminPinCommand(id), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("orders")]
    public async Task<ActionResult<AdminPromoterOrdersPage>> Orders([FromQuery] int? branchId, [FromQuery] int? userId,
        [FromQuery] string? stage, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int page = 1)
        => (await _mediator.Send(new GetAdminPromoterOrdersQuery(branchId, userId, stage, from, to, page), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("orders/{id:int}/shop-photo")]
    public async Task<IActionResult> ShopPhoto(int id)
    {
        var result = await _mediator.Send(new GetPromoterOrderShopPhotoQuery(id), CancellationToken);
        if (result.IsFailed) return result.ToHttpResponse().Result!;
        return File(result.Data!.Data, result.Data.ContentType);
    }

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("promoters")]
    public async Task<ActionResult<List<PromoterAccountDto>>> Promoters([FromQuery] string? q, [FromQuery] string? status, [FromQuery] int? branchId)
        => (await _mediator.Send(new GetPromotersQuery(q, status, branchId), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Manage)]
    [HttpPost("promoters")]
    public async Task<ActionResult<int>> CreatePromoter([FromBody] SavePromoterCommand command)
        => (await _mediator.Send(command with { Id = null }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Manage)]
    [HttpPut("promoters/{id:int}")]
    public async Task<ActionResult<int>> UpdatePromoter(int id, [FromBody] SavePromoterCommand command)
        => (await _mediator.Send(command with { Id = id, Password = null }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Manage)]
    [HttpPost("promoters/{id:int}/password")]
    public async Task<ActionResult> SetPassword(int id, [FromBody] PasswordBody body)
        => (await _mediator.Send(new SetPromoterPasswordCommand(id, body.Password), CancellationToken)).ToHttpResponse();

    public sealed record PasswordBody(string Password);

    [HasPermission(Permissions.FieldPromoter.Manage)]
    [HttpPost("promoters/{id:int}/suspend")]
    public async Task<ActionResult> Suspend(int id, [FromBody] SuspendBody body)
        => (await _mediator.Send(new SuspendPromoterCommand(id, body.Suspended, body.Reason), CancellationToken)).ToHttpResponse();

    public sealed record SuspendBody(bool Suspended, string? Reason);

    [HasPermission(Permissions.FieldPromoter.Manage)]
    [HttpPut("promoters/{id:int}/target")]
    public async Task<ActionResult> Target(int id, [FromBody] TargetBody body)
        => (await _mediator.Send(new SetPromoterTargetCommand(id, body.Minutes), CancellationToken)).ToHttpResponse();

    public sealed record TargetBody(int? Minutes);

    [HasPermission(Permissions.FieldPromoter.Read)]
    [HttpGet("settings")]
    public async Task<ActionResult<PromoterSettingsDto>> Settings()
        => (await _mediator.Send(new GetPromoterSettingsQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.FieldPromoter.Manage)]
    [HttpPut("settings")]
    public async Task<ActionResult<PromoterSettingsDto>> SaveSettings([FromBody] PromoterSettingsDto settings)
        => (await _mediator.Send(new SavePromoterSettingsCommand(settings), CancellationToken)).ToHttpResponse();
}
