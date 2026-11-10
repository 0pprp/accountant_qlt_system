using Application.Common.Interfaces;
using Application.LegalFollowups.Commands.Admin.Cancel;
using Application.LegalFollowups.Commands.Admin.Create;
using Application.LegalFollowups.Common;
using Application.LegalFollowups.Queries.Admin.GetById;
using Application.LegalFollowups.Queries.Admin.List;
using Application.LegalFollowups.Queries.Admin.ListFollowers;
using Application.SalesRequests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/LegalFollowups")]
public class LegalFollowupsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ICurrentUserService _current;

    public LegalFollowupsController(
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

    [HasPermission(Permissions.LegalFollowup.Read)]
    [HttpGet]
    public async Task<ActionResult<ListLegalFollowupsResponse>> List(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(
            new ListLegalFollowupsQuery
            {
                BranchId = branchId,
                Status = status,
                Search = search,
                Page = page,
                PageSize = pageSize,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>قبل {{id}} حتى لا يُفسَّر followers كرقم.</summary>
    [HasPermission(Permissions.LegalFollowup.Create)]
    [HttpGet("followers")]
    public async Task<ActionResult<IReadOnlyList<LegalFollowupFollowerOptionDto>>> Followers(
        [FromQuery] int? branchId)
    {
        var result = await _mediator.Send(
            new ListLegalFollowupFollowersQuery { BranchId = branchId },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.LegalFollowup.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LegalFollowupCaseRowDto>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetLegalFollowupByIdQuery { CaseId = id }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.LegalFollowup.Create)]
    [HttpPost]
    public async Task<ActionResult<LegalFollowupCaseRowDto>> Create([FromBody] CreateLegalFollowupBody? body)
    {
        body ??= new CreateLegalFollowupBody();
        var result = await _mediator.Send(
            new CreateLegalFollowupCommand
            {
                SalesRequestId = body.SalesRequestId,
                FollowerUserId = body.FollowerUserId,
                Note = body.Note,
                ExpectedConcurrencyStamp = body.ExpectedConcurrencyStamp,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>
    /// إلغاء متابعة قانونية قيد العمل — يعيد الطلب لغير مسند إن كان LegalFollowup.
    /// لا يحذف الزبون ولا الدين. صلاحية LegalFollowup.Cancel مستقلة عن القراءة.
    /// </summary>
    [HasPermission(Permissions.LegalFollowup.Cancel)]
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<CancelLegalFollowupResponse>> Cancel(
        [FromRoute] int id,
        [FromBody] CancelLegalFollowupBody? body)
    {
        body ??= new CancelLegalFollowupBody();
        var result = await _mediator.Send(
            new CancelLegalFollowupCommand
            {
                CaseId = id,
                Reason = body.Reason ?? "",
                ExpectedVersion = body.ExpectedVersion
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>معاينة/تنزيل مستمسك ضمن نطاق الفرع — LegalFollowup.Read فقط؛ لا مسار مكشوف.</summary>
    [HasPermission(Permissions.LegalFollowup.Read)]
    [HttpGet("{id:int}/documents/{docId:int}/file")]
    public async Task<IActionResult> DownloadDocument([FromRoute] int id, [FromRoute] int docId)
    {
        var scope = SalesRequestAccess.AuthorizedBranches(_current);
        if (scope.Count == 0)
            return Forbid();

        var entity = await _db.LegalFollowupCases.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, CancellationToken);
        if (entity is null)
            return NotFound();

        int? branchId = null;
        if (entity.SalesRequestId is int srId)
        {
            branchId = await _db.SalesRequests.AsNoTracking()
                .Where(r => r.Id == srId)
                .Select(r => (int?)r.BranchId)
                .FirstOrDefaultAsync(CancellationToken);
        }

        if (branchId is null || scope.Contains(branchId.Value) == false)
            return NotFound();

        var doc = await _db.LegalFollowupCaseDocuments.AsNoTracking()
            .Where(d => d.Id == docId && d.CaseId == id)
            .Select(d => new { d.RelativePath, d.ContentType, d.FileName })
            .FirstOrDefaultAsync(CancellationToken);
        if (doc is null)
            return NotFound();

        var fullPath = Path.Combine(_env.WebRootPath ?? "", doc.RelativePath);
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, doc.ContentType, doc.FileName);
    }

    public sealed class CancelLegalFollowupBody
    {
        public string? Reason { get; set; }
        public int? ExpectedVersion { get; set; }
    }

    public sealed class CreateLegalFollowupBody
    {
        public int SalesRequestId { get; set; }
        public int FollowerUserId { get; set; }
        public string? Note { get; set; }
        public int? ExpectedConcurrencyStamp { get; set; }
    }
}
