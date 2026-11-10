using Application.FollowerWork.Common;
using Application.FollowerWork.Queries.Admin.GetCollection;
using Application.FollowerWork.Queries.Admin.GetDeclining;
using Application.FollowerWork.Queries.Admin.GetDelegateNotes;
using Application.FollowerWork.Queries.Admin.GetFollowers;
using Application.FollowerWork.Queries.Admin.GetNotes;
using Application.FollowerWork.Queries.Admin.GetOldCustomers;
using Application.FollowerWork.Queries.Admin.GetRemainingCustomers;
using Application.FollowerWork.Queries.Admin.GetSalesReview;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class FollowerWorkController : ApiController
{
    private readonly IMediator _mediator;

    public FollowerWorkController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// دليل بطاقات المتابعين. التفويض: FollowerWork.Read أو صلاحية قسم يحتاج الدليل (يُفحص في الـhandler).
    /// لا يُرجع عدادات أقسام غير منفّذة ولا حقولًا حساسة للحساب.
    /// </summary>
    [Authorize]
    [HttpGet("followers")]
    public async Task<ActionResult<FollowerWorkFollowersResponse>> GetFollowers(
        [FromQuery] int? branchId = null,
        [FromQuery] string? search = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sort = null)
    {
        var result = await _mediator.Send(
            new GetFollowerWorkFollowersQuery
            {
                BranchId = branchId,
                Search = search,
                Page = page,
                PageSize = pageSize,
                Sort = sort
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>قراءة مصادقة البيع للمتابع — بلا كتابة من اللوحة.</summary>
    [HasPermission(Permissions.FollowerWork.SalesReviewRead)]
    [HttpGet("{followerId:int}/sales-review")]
    public async Task<ActionResult<FollowerSalesReviewWorkspaceDto>> GetSalesReview(
        [FromRoute] int followerId,
        [FromQuery] int? branchId = null)
    {
        var result = await _mediator.Send(
            new GetFollowerSalesReviewQuery
            {
                FollowerId = followerId,
                BranchId = branchId
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.FollowerWork.SalesReviewRead)]
    [HttpGet("{followerId:int}/remaining-customers")]
    public async Task<ActionResult<FollowerRemainingCustomersResponse>> GetRemainingCustomers(
        [FromRoute] int followerId,
        [FromQuery] int? branchId = null,
        [FromQuery] string? bucket = null)
    {
        var result = await _mediator.Send(
            new GetFollowerRemainingCustomersQuery
            {
                FollowerId = followerId,
                BranchId = branchId,
                Bucket = bucket
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>
    /// زبائن متدنية للمتابع — قراءة فقط. التعريف: هبوط درجتين خلال 7 أيام على انتقالات محددة.
    /// </summary>
    [HasPermission(Permissions.FollowerWork.DecliningRead)]
    [HttpGet("{followerId:int}/declining")]
    public async Task<ActionResult<FollowerDecliningResponse>> GetDeclining(
        [FromRoute] int followerId,
        [FromQuery] int? branchId = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        var result = await _mediator.Send(
            new GetFollowerDecliningQuery
            {
                FollowerId = followerId,
                BranchId = branchId,
                Page = page,
                PageSize = pageSize
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>ملاحظات زبون المتابع — قراءة فقط من اللوحة.</summary>
    [HasPermission(Permissions.FollowerWork.NotesRead)]
    [HttpGet("{followerId:int}/notes")]
    public async Task<ActionResult<FollowerNotesResponse>> GetNotes(
        [FromRoute] int followerId,
        [FromQuery] int? branchId = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        var result = await _mediator.Send(
            new GetFollowerNotesQuery
            {
                FollowerId = followerId,
                BranchId = branchId,
                Page = page,
                PageSize = pageSize
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>الزبائن القديمة = قضايا LegalFollowupCases للمتابع — ليست IsLegal وحدها.</summary>
    [HasPermission(Permissions.FollowerWork.OldCustomersRead)]
    [HttpGet("{followerId:int}/old-customers")]
    public async Task<ActionResult<FollowerOldCustomersResponse>> GetOldCustomers(
        [FromRoute] int followerId,
        [FromQuery] int? branchId = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        var result = await _mediator.Send(
            new GetFollowerOldCustomersQuery
            {
                FollowerId = followerId,
                BranchId = branchId,
                Page = page,
                PageSize = pageSize
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>
    /// ملاحظات المندوبين = FollowerDelegateNotes (يكتبها المتابع عن القائمة/المندوب).
    /// ليست CustomerNotes. اللوحة قراءة فقط.
    /// </summary>
    [HasPermission(Permissions.FollowerWork.DelegateNotesRead)]
    [HttpGet("{followerId:int}/delegate-notes")]
    public async Task<ActionResult<FollowerDelegateNotesResponse>> GetDelegateNotes(
        [FromRoute] int followerId,
        [FromQuery] int? branchId = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        var result = await _mediator.Send(
            new GetFollowerDelegateNotesQuery
            {
                FollowerId = followerId,
                BranchId = branchId,
                Page = page,
                PageSize = pageSize
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>
    /// جباية المتابع: قوائم + تمكين + مدفوعات InstallmentPayments مع مجاميع DB.
    /// قراءة لا تمنح ManageEnabled ولا إنشاء دفعة.
    /// </summary>
    [HasPermission(Permissions.FollowerWork.CollectionRead)]
    [HttpGet("{followerId:int}/collection")]
    public async Task<ActionResult<FollowerCollectionResponse>> GetCollection(
        [FromRoute] int followerId,
        [FromQuery] int? branchId = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] int? paymentsPage = null,
        [FromQuery] int? paymentsPageSize = null)
    {
        var result = await _mediator.Send(
            new GetFollowerCollectionQuery
            {
                FollowerId = followerId,
                BranchId = branchId,
                FromDate = fromDate,
                ToDate = toDate,
                PaymentsPage = paymentsPage,
                PaymentsPageSize = paymentsPageSize
            },
            CancellationToken);
        return result.ToHttpResponse();
    }
}

