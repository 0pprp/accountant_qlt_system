using Api.Models;
using Application.Common;
using Application.DelegateFollowup.Common;
using Application.FollowerWork.Common;
using Application.ManagementTasks.Common;
using Application.Orders.Common;
using Application.SalesRequests.Common;
using Application.Users.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToHttpResponse(this Result result)
    {
        if (result.IsSucceeded)
            return new OkResult();

        if (IsConflict(result.Error))
            return new ConflictObjectResult(new BadRequestBody(result.Error));

        if (IsForbidden(result.Error))
            return new ObjectResult(new BadRequestBody(result.Error)) { StatusCode = StatusCodes.Status403Forbidden };

        return new BadRequestObjectResult(new BadRequestBody(result.Error));
    }

    public static ActionResult<TData> ToHttpResponse<TData>(this Result<TData> result)
    {
        if (result.IsSucceeded)
            return new OkObjectResult(result.Data);

        if (IsConflict(result.Error))
            return new ConflictObjectResult(new BadRequestBody(result.Error));

        if (IsForbidden(result.Error))
            return new ObjectResult(new BadRequestBody(result.Error)) { StatusCode = StatusCodes.Status403Forbidden };

        return new BadRequestObjectResult(new BadRequestBody(result.Error));
    }

    private static bool IsConflict(Error? error) =>
        error?.Code == UserErrors.PermissionsVersionConflict.Code
        || error?.Code == SalesRequestErrors.ConcurrencyConflict.Code
        || error?.Code == ManagementTaskErrors.ConcurrencyConflict.Code
        || error?.Code == FollowerWorkErrors.CollectionConcurrencyConflict.Code
        || error?.Code == OrderErrors.ClientPaymentPayloadConflict.Code;

    private static bool IsForbidden(Error? error) =>
        error?.Code == SalesRequestErrors.TabForbidden.Code
        || error?.Code == SalesRequestErrors.BranchForbidden.Code
        || error?.Code == SalesRequestErrors.ProfileForbidden.Code
        || error?.Code == SalesRequestErrors.DocumentsForbidden.Code
        || error?.Code == ManagementTaskErrors.ActionForbidden.Code
        || error?.Code == ManagementTaskErrors.BranchForbidden.Code
        || error?.Code == ManagementTaskErrors.AttachmentForbidden.Code
        || error?.Code == DelegateFollowupErrors.BranchForbidden.Code
        || error?.Code == DelegateFollowupErrors.ActorRequired.Code
        || error?.Code == FollowerWorkErrors.CollectionManageDenied.Code
        || error?.Code == FollowerWorkErrors.CollectionListOutOfScope.Code
        || error?.Code == FollowerWorkErrors.MandobCollectionLocked.Code
        || error?.Code == FollowerWorkErrors.DelegateNotesWriteDenied.Code
        || error?.Code == FollowerWorkErrors.DelegateNotesListOutOfScope.Code;
}
