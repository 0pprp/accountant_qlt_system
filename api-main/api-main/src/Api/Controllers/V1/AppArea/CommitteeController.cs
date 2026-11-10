using Application.AppNotes.Common;
using Application.Committee.Commands.App.AddCustomerNote;
using Application.Committee.Commands.App.AddDelegateNote;
using Application.Committee.Commands.App.AuthenticatePendingSale;
using Application.Committee.Commands.App.FieldFollowups;
using Application.Committee.Commands.App.PostComplaint;
using Application.FollowerWork.Common;
using Application.Committee.Commands.App.PostDayRating;
using Application.Committee.Commands.App.PostDirectorNote;
using Application.Committee.Common;
using Application.Committee.Queries.App.GetCollectionLists;
using Application.Committee.Queries.App.GetDaily;
using Application.Committee.Queries.App.GetDecliningCustomers;
using Application.Committee.Queries.App.GetFieldFollowups;
using Application.Committee.Queries.App.GetListCards;
using Application.Committee.Queries.App.GetLists;
using Application.Committee.Queries.App.GetListStats;
using Application.Committee.Queries.App.GetMe;
using Application.Committee.Queries.App.GetPendingSales;
using Application.CommitteeTasks.Common;
using Application.CommitteeTasks.Queries;
using Application.SalesLocations.Commands.App.IngestBatch;
using Application.SalesLocations.Commands.App.IngestLive;
using Application.SalesShifts.Commands.App.End;
using Application.SalesShifts.Commands.App.Start;
using Application.SalesShifts.Queries.App.GetCurrent;

namespace Api.Controllers.V1.AppArea;

/// <summary>AppArea لمسؤول اللجنة — JWT + صلاحيات، مسارات أسرع من Followers/* القديم.</summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/committee")]
public class CommitteeController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _current;

    public CommitteeController(IMediator mediator, ICurrentUserService current)
    {
        _mediator = mediator;
        _current = current;
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("me")]
    public async Task<ActionResult<GetCommitteeMeResponse>> Me()
    {
        var result = await _mediator.Send(
            new GetCommitteeMeQuery(_current.UserId!.Value, _current.Role),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("lists")]
    public async Task<ActionResult<List<CommitteeListDto>>> Lists()
    {
        var result = await _mediator.Send(new GetCommitteeListsQuery(_current.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("list-cards")]
    public async Task<ActionResult<List<CommitteeListCardDto>>> ListCards()
    {
        var result = await _mediator.Send(new GetCommitteeListCardsQuery(_current.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("daily")]
    public async Task<ActionResult<CommitteeDailyResponse>> Daily(
        [FromQuery] int listId,
        [FromQuery] DateOnly? date,
        [FromQuery] string showType = "المسددين")
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _mediator.Send(
            new GetCommitteeDailyQuery(_current.UserId!.Value, listId, d, showType),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("collection-lists")]
    public async Task<ActionResult<List<CommitteeCollectionListDto>>> CollectionLists()
    {
        var result = await _mediator.Send(
            new GetCommitteeCollectionListsQuery(_current.UserId!.Value),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("declining-customers")]
    public async Task<ActionResult<List<CommitteeDecliningCustomerDto>>> DecliningCustomers()
    {
        var result = await _mediator.Send(
            new GetCommitteeDecliningCustomersQuery(_current.UserId!.Value),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("pending-sales")]
    public async Task<ActionResult<List<CommitteePendingSaleDto>>> PendingSales()
    {
        var result = await _mediator.Send(
            new GetCommitteePendingSalesQuery(_current.UserId!.Value),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.FollowerWork.SalesReviewAuthenticate)]
    [HttpPost("pending-sales/{orderId:int}/authenticate")]
    public async Task<ActionResult> AuthenticatePendingSale(
        [FromRoute] int orderId,
        [FromBody] AuthenticateCommitteePendingSaleBody? body)
    {
        body ??= new AuthenticateCommitteePendingSaleBody();
        var result = await _mediator.Send(
            new AuthenticateCommitteePendingSaleCommand(
                _current.UserId!.Value,
                orderId,
                body.Rating,
                body.Note,
                body.ShopStockEstimatedValue,
                body.EstimatedDailyRevenue),
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>ملاحظة زبون ميدانية — CustomerNotes؛ ليست ملاحظة مندوب ولا ملاحظة للمدير.</summary>
    [HasPermission(Permissions.FollowerWork.NotesWrite)]
    [HttpPost("customers/{customerId:int}/notes")]
    public async Task<ActionResult<FollowerNoteRowDto>> AddCustomerNote(
        [FromRoute] int customerId,
        [FromBody] AddCommitteeCustomerNoteBody? body)
    {
        body ??= new AddCommitteeCustomerNoteBody();
        var result = await _mediator.Send(
            new AddCommitteeCustomerNoteCommand(
                _current.UserId!.Value,
                customerId,
                body.ListId,
                body.NoteText ?? ""),
            CancellationToken);
        return result.ToHttpResponse();
    }

    public sealed class AddCommitteeCustomerNoteBody
    {
        public int ListId { get; set; }
        public string? NoteText { get; set; }
    }

    /// <summary>
    /// ملاحظة مندوب/قائمة — FollowerDelegateNotes؛ الكاتب = جلسة المتابع؛ غير قابلة للتعديل.
    /// </summary>
    [HasPermission(Permissions.FollowerWork.DelegateNotesWrite)]
    [HttpPost("order-lists/{listId:int}/delegate-notes")]
    public async Task<ActionResult<FollowerDelegateNoteRowDto>> AddDelegateNote(
        [FromRoute] int listId,
        [FromBody] AddCommitteeDelegateNoteBody? body)
    {
        body ??= new AddCommitteeDelegateNoteBody();
        var result = await _mediator.Send(
            new AddCommitteeDelegateNoteCommand(
                _current.UserId!.Value,
                listId,
                body.NoteText ?? ""),
            CancellationToken);
        return result.ToHttpResponse();
    }

    public sealed class AddCommitteeDelegateNoteBody
    {
        public string? NoteText { get; set; }
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("day-ratings/missing")]
    public async Task<ActionResult<List<CommitteeMissingDayRatingDto>>> MissingDayRatings([FromQuery] DateOnly? date)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _mediator.Send(
            new GetCommitteeMissingDayRatingsQuery(_current.UserId!.Value, d),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("day-ratings")]
    public async Task<ActionResult> PostDayRating([FromBody] PostCommitteeDayRatingDto dto)
    {
        var result = await _mediator.Send(new PostCommitteeDayRatingCommand
        {
            UserId = _current.UserId!.Value,
            ListId = dto.ListId,
            Date = dto.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Score = dto.Score,
            Note = dto.Note
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("complaints")]
    public async Task<ActionResult> Complaint([FromBody] PostCommitteeComplaintDto dto)
    {
        var result = await _mediator.Send(new PostCommitteeComplaintCommand
        {
            UserId = _current.UserId!.Value,
            Body = dto.Body,
            Subject = dto.Subject
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.CommitteeTask.AppRead)]
    [HttpGet("tasks")]
    public async Task<ActionResult<IReadOnlyList<CommitteeTaskAppItemDto>>> Tasks()
        => (await _mediator.Send(new ListAppCommitteeTasksQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.AppNote.Create)]
    [HttpPost("director-notes")]
    public async Task<ActionResult<AppNoteDetailDto>> DirectorNote([FromBody] PostCommitteeDirectorNoteDto dto)
    {
        var result = await _mediator.Send(new PostCommitteeDirectorNoteCommand
        {
            Body = dto.Body,
            Subject = dto.Subject
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>متابعات ميدانية معلّقة للمتابع الحالي — ليست LegalFollowup ولا ManagementTask.</summary>
    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("field-followups")]
    public async Task<ActionResult<List<CommitteeFieldFollowupDto>>> FieldFollowups()
    {
        var result = await _mediator.Send(
            new GetCommitteeFieldFollowupsQuery(_current.UserId!.Value),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("field-followups")]
    public async Task<ActionResult<CommitteeFieldFollowupDto>> AddFieldFollowup(
        [FromBody] AddCommitteeFieldFollowupBody? body)
    {
        body ??= new AddCommitteeFieldFollowupBody();
        var result = await _mediator.Send(
            new AddCommitteeFieldFollowupCommand(
                _current.UserId!.Value,
                body.ListId,
                body.ListName,
                body.CustomerId,
                body.CustomerName,
                body.PhoneNumber),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("field-followups/{id:int}/complete")]
    public async Task<ActionResult<CommitteeFieldFollowupDto>> CompleteFieldFollowup(
        [FromRoute] int id,
        [FromBody] CompleteCommitteeFieldFollowupBody? body)
    {
        body ??= new CompleteCommitteeFieldFollowupBody();
        var result = await _mediator.Send(
            new CompleteCommitteeFieldFollowupCommand(_current.UserId!.Value, id, body.Note),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("field-followups/record-complete")]
    public async Task<ActionResult<CommitteeFieldFollowupDto>> RecordCompleteFieldFollowup(
        [FromBody] RecordCompleteCommitteeFieldFollowupBody? body)
    {
        body ??= new RecordCompleteCommitteeFieldFollowupBody();
        var result = await _mediator.Send(
            new RecordCompleteCommitteeFieldFollowupCommand(
                _current.UserId!.Value,
                body.ListId,
                body.ListName,
                body.CustomerId,
                body.CustomerName,
                body.PhoneNumber,
                body.Note),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("field-followups/latest-notes")]
    public async Task<ActionResult<List<CommitteeFieldFollowupLatestNoteDto>>> FieldFollowupLatestNotes(
        [FromQuery] int listId)
    {
        var result = await _mediator.Send(
            new GetCommitteeFieldFollowupLatestNotesQuery(_current.UserId!.Value, listId),
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>إحصائيات قوائم المتابع — معادلات ListStats المرجعية؛ ليست ملخص مبيعات عام.</summary>
    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("list-stats")]
    public async Task<ActionResult<CommitteeListStatsResponse>> ListStats()
    {
        var result = await _mediator.Send(
            new GetCommitteeListStatsQuery(_current.UserId!.Value),
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("list-stats/{listId:int}")]
    public async Task<ActionResult<CommitteeListStatsRowDto>> ListStatsOne([FromRoute] int listId)
    {
        var result = await _mediator.Send(
            new GetCommitteeListStatsOneQuery(_current.UserId!.Value, listId),
            CancellationToken);
        return result.ToHttpResponse();
    }

    // —— دوام وتتبع (يعيد استخدام SalesShift / SalesLocation بأمان JWT) ——

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("shifts/start")]
    public async Task<ActionResult<StartSalesShiftResponse>> StartShift()
    {
        var result = await _mediator.Send(new StartSalesShiftCommand(_current.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("shifts/end")]
    public async Task<ActionResult<EndSalesShiftResponse>> EndShift()
    {
        var result = await _mediator.Send(new EndSalesShiftCommand(_current.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("shifts/current")]
    public async Task<ActionResult<GetCurrentSalesShiftResponse>> CurrentShift()
    {
        var result = await _mediator.Send(new GetCurrentSalesShiftQuery(_current.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("locations/batch")]
    public async Task<ActionResult<IngestSalesLocationBatchResponse>> LocationsBatch(
        [FromBody] List<SalesLocationPointDto> points)
    {
        var result = await _mediator.Send(new IngestSalesLocationBatchCommand
        {
            UserId = _current.UserId!.Value,
            Points = points,
            ActorKind = Domain.Entities.SalesLocationAggregate.Enums.SalesLocationActorKind.Follower,
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpPost("locations/live")]
    public async Task<ActionResult<IngestSalesLiveLocationResponse>> LocationsLive(
        [FromBody] IngestSalesLiveLocationDto dto)
    {
        var result = await _mediator.Send(new IngestSalesLiveLocationCommand
        {
            UserId = _current.UserId!.Value,
            Lat = dto.Lat,
            Lng = dto.Lng,
            AccuracyMeters = dto.AccuracyMeters ?? dto.Accuracy,
            CapturedAtUtc = dto.CapturedAtUtc,
            ClientEventId = dto.ClientEventId,
            ActorKind = Domain.Entities.SalesLocationAggregate.Enums.SalesLocationActorKind.Follower,
        }, CancellationToken);
        return result.ToHttpResponse();
    }
}

public sealed class AuthenticateCommitteePendingSaleBody
{
    public int Rating { get; set; }
    public string? Note { get; set; }
    public decimal ShopStockEstimatedValue { get; set; }
    public decimal EstimatedDailyRevenue { get; set; }
}

public sealed class AddCommitteeFieldFollowupBody
{
    public int ListId { get; set; }
    public string? ListName { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? PhoneNumber { get; set; }
}

public sealed class CompleteCommitteeFieldFollowupBody
{
    public string? Note { get; set; }
}

public sealed class RecordCompleteCommitteeFieldFollowupBody
{
    public int ListId { get; set; }
    public string? ListName { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Note { get; set; }
}

public record PostCommitteeDayRatingDto
{
    public int ListId { get; set; }
    public DateOnly? Date { get; set; }
    public int Score { get; set; }
    public string? Note { get; set; }
}

public record PostCommitteeComplaintDto
{
    public required string Body { get; set; }
    public string? Subject { get; set; }
}

public record PostCommitteeDirectorNoteDto
{
    public required string Body { get; set; }
    public string? Subject { get; set; }
}
