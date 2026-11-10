using Application.Common.Interfaces;
using Application.LegalFollowups.Commands.App.Complete;
using Application.LegalFollowups.Commands.App.SetLocation;
using Application.LegalFollowups.Commands.App.UploadDocument;
using Application.LegalFollowups.Common;
using Application.LegalFollowups.Queries.App.GetMine;
using Application.LegalFollowups.Queries.App.ListMine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.V1.AppArea;

/// <summary>متابعة قانونية ميدانية للمتابع (Motaba) — AppArea.</summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/committee/legal-followups")]
public class CommitteeLegalFollowupsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ICurrentUserService _current;

    public CommitteeLegalFollowupsController(
        IMediator mediator,
        IApplicationDbContext db,
        IWebHostEnvironment env,
        ICurrentUserService current)
    {
        _mediator = mediator;
        _db = db;
        _env = env;
        _current = current;
    }

    [HasPermission(Permissions.LegalFollowup.FieldRead)]
    [HttpGet]
    public async Task<ActionResult<ListLegalFollowupsResponse>> List(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(
            new ListMyLegalFollowupsQuery { Status = status, Page = page, PageSize = pageSize },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.LegalFollowup.FieldRead)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LegalFollowupCaseRowDto>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetMyLegalFollowupByIdQuery { CaseId = id }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.LegalFollowup.FieldUpload)]
    [HttpPost("{id:int}/location")]
    public async Task<ActionResult<LegalFollowupCaseRowDto>> SetLocation(
        [FromRoute] int id,
        [FromBody] LocationBody? body)
    {
        body ??= new LocationBody();
        var result = await _mediator.Send(
            new SetLegalFollowupLocationCommand
            {
                CaseId = id,
                Latitude = body.Latitude,
                Longitude = body.Longitude,
                ExpectedVersion = body.ExpectedVersion,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.LegalFollowup.FieldUpload)]
    [HttpPost("{id:int}/documents")]
    [RequestSizeLimit(LegalFollowupCompletionRules.MaxDocumentBytes + 1024)]
    public async Task<ActionResult<LegalFollowupDocumentDto>> UploadDocument(
        [FromRoute] int id,
        [FromForm] string documentType,
        IFormFile? file)
    {
        var result = await _mediator.Send(
            new UploadLegalFollowupDocumentCommand
            {
                CaseId = id,
                DocumentType = documentType,
                File = file,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.LegalFollowup.FieldComplete)]
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<LegalFollowupCaseRowDto>> Complete(
        [FromRoute] int id,
        [FromBody] CompleteBody? body)
    {
        body ??= new CompleteBody();
        var result = await _mediator.Send(
            new CompleteLegalFollowupCommand
            {
                CaseId = id,
                Note = body.Note,
                ExpectedVersion = body.ExpectedVersion,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.LegalFollowup.FieldRead)]
    [HttpGet("{id:int}/documents/{docId:int}/file")]
    public async Task<IActionResult> DownloadDocument([FromRoute] int id, [FromRoute] int docId)
    {
        if (_current.UserId is not int followerId)
            return Forbid();

        var owns = await _db.LegalFollowupCases.AsNoTracking()
            .AnyAsync(c => c.Id == id && c.FollowerUserId == followerId, CancellationToken);
        if (!owns)
            return NotFound();

        var doc = await _db.LegalFollowupCaseDocuments.AsNoTracking()
            .Where(d => d.Id == docId && d.CaseId == id)
            .Select(d => new { d.RelativePath, d.ContentType, d.FileName })
            .FirstOrDefaultAsync(CancellationToken);
        if (doc is null)
            return NotFound();

        var fullPath = Path.Combine(_env.WebRootPath, doc.RelativePath);
        if (!System.IO.File.Exists(fullPath))
            return NotFound();
        return PhysicalFile(fullPath, doc.ContentType, doc.FileName);
    }

    public sealed class LocationBody
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? ExpectedVersion { get; set; }
    }

    public sealed class CompleteBody
    {
        public string? Note { get; set; }
        public int? ExpectedVersion { get; set; }
    }
}
