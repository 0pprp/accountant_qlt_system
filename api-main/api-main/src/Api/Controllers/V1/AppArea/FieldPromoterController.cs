using Api.Authorization;
using Api.Extensions;
using Application.Common.SeedData;
using Application.FieldPromoters.App;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AppArea;

/// <summary>تطبيق المروّج الميداني.</summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/field-promoter")]
[HasPermission(Permissions.FieldPromoter.App)]
public class FieldPromoterController(IMediator mediator) : ApiController
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("me")]
    public async Task<ActionResult<PromoterMeDto>> Me()
        => (await _mediator.Send(new GetPromoterMeQuery(), CancellationToken)).ToHttpResponse();

    [HttpPost("shift/start")]
    public async Task<ActionResult<PromoterShiftDto>> StartShift()
        => (await _mediator.Send(new StartPromoterShiftCommand(), CancellationToken)).ToHttpResponse();

    [HttpPost("shift/end")]
    public async Task<ActionResult<PromoterShiftDto>> EndShift()
        => (await _mediator.Send(new EndPromoterShiftCommand(), CancellationToken)).ToHttpResponse();

    [HttpGet("today")]
    public async Task<ActionResult<PromoterTodayDto>> Today()
        => (await _mediator.Send(new GetPromoterTodayQuery(), CancellationToken)).ToHttpResponse();

    [HttpPost("locations/batch")]
    [RequestSizeLimit(512 * 1024)]
    public async Task<ActionResult<PromoterIngestResult>> Locations([FromBody] List<PromoterPointInput> points)
        => (await _mediator.Send(new IngestPromoterLocationsCommand(points ?? []), CancellationToken)).ToHttpResponse();

    [HttpPost("orders")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<ActionResult<PromoterOrderReceipt>> SubmitOrder([FromForm] SubmitOrderForm form)
    {
        byte[]? photo = null;
        if (form.Photo is { Length: > 0 })
        {
            await using var stream = new MemoryStream();
            await form.Photo.CopyToAsync(stream, CancellationToken);
            photo = stream.ToArray();
        }
        return (await _mediator.Send(new SubmitPromoterOrderCommand
        {
            SubmissionKey = form.SubmissionKey, BranchId = form.BranchId, CustomerName = form.CustomerName ?? "",
            CustomerPhone = form.CustomerPhone ?? "", Product = form.Product ?? "", Address = form.Address ?? "",
            Note = form.Note, Latitude = form.Latitude, Longitude = form.Longitude, Photo = photo
        }, CancellationToken)).ToHttpResponse();
    }

    public sealed class SubmitOrderForm
    {
        public Guid SubmissionKey { get; set; }
        public int BranchId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? Product { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public IFormFile? Photo { get; set; }
    }

    [HttpGet("orders")]
    public async Task<ActionResult<PromoterOrdersPage>> Orders([FromQuery] string tab = "inProgress", [FromQuery] int page = 1)
        => (await _mediator.Send(new GetPromoterOrdersQuery(tab, page), CancellationToken)).ToHttpResponse();

    [HttpGet("orders/counts")]
    public async Task<ActionResult<PromoterOrderCounts>> OrderCounts()
        => (await _mediator.Send(new GetPromoterOrderCountsQuery(), CancellationToken)).ToHttpResponse();

    [HttpGet("orders/{id:int}")]
    public async Task<ActionResult<PromoterOrderDto>> Order(int id)
        => (await _mediator.Send(new GetPromoterOrderQuery(id), CancellationToken)).ToHttpResponse();

    [HttpPost("orders/{id:int}/cancel")]
    public async Task<ActionResult> CancelOrder(int id, [FromBody] CancelBody? body)
        => (await _mediator.Send(new CancelPromoterOrderCommand(id, body?.Reason), CancellationToken)).ToHttpResponse();

    public sealed record CancelBody(string? Reason);

    [HttpGet("pins")]
    public async Task<ActionResult<List<PinMapDto>>> PinsInBox([FromQuery] double minLat, [FromQuery] double minLng,
        [FromQuery] double maxLat, [FromQuery] double maxLng)
        => (await _mediator.Send(new GetPinsInBoxQuery(minLat, minLng, maxLat, maxLng), CancellationToken)).ToHttpResponse();

    [HttpGet("pins/mine")]
    public async Task<ActionResult<PinsPage>> MyPins([FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] string? q, [FromQuery] int page = 1)
        => (await _mediator.Send(new GetMyPinsQuery(from, to, q, page), CancellationToken)).ToHttpResponse();

    [HttpGet("pins/{id:int}")]
    public async Task<ActionResult<PinDto>> Pin(int id)
        => (await _mediator.Send(new GetPinQuery(id), CancellationToken)).ToHttpResponse();

    [HttpPost("pins")]
    public async Task<ActionResult<PinDto>> CreatePin([FromBody] PinInput input)
        => (await _mediator.Send(new CreatePinCommand(input), CancellationToken)).ToHttpResponse();

    [HttpPut("pins/{id:int}")]
    public async Task<ActionResult<PinDto>> UpdatePin(int id, [FromBody] PinInput input)
        => (await _mediator.Send(new UpdatePinCommand(id, input), CancellationToken)).ToHttpResponse();

    [HttpDelete("pins/{id:int}")]
    public async Task<ActionResult> DeletePin(int id)
        => (await _mediator.Send(new DeletePinCommand(id), CancellationToken)).ToHttpResponse();
}
