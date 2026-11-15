using Application.ManagementSalesMap.Common;
using Application.ManagementSalesMap.Queries.Admin.GetLists;
using Application.ManagementSalesMap.Queries.Admin.GetOrderDetail;
using Application.ManagementSalesMap.Queries.Admin.GetPoints;
using Application.ManagementSalesMap.Queries.Admin.GetProvinces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/ManagementSalesMap")]
public class ManagementSalesMapController : ApiController
{
    private readonly IMediator _mediator;

    public ManagementSalesMapController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.ManagementSalesMap.Read)]
    [HttpGet("provinces")]
    public async Task<ActionResult<SalesMapProvincesResponse>> Provinces()
    {
        var result = await _mediator.Send(new GetSalesMapProvincesQuery(), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementSalesMap.Read)]
    [HttpGet("provinces/{provinceId:int}/lists")]
    public async Task<ActionResult<SalesMapListsResponse>> Lists([FromRoute] int provinceId)
    {
        var result = await _mediator.Send(
            new GetSalesMapListsQuery { ProvinceId = provinceId },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementSalesMap.Read)]
    [HttpGet("provinces/{provinceId:int}/points")]
    public async Task<ActionResult<SalesMapPointsResponse>> Points(
        [FromRoute] int provinceId,
        [FromQuery] int[]? listIds,
        [FromQuery] SalesMapStatusFilter status = SalesMapStatusFilter.All)
    {
        // ASP.NET يربط المصفوفة الغائبة كمصفوفة فارغة وليس null — الفارغ/الغائب = كل القوائم المؤهلة.
        // الواجهة لا تستدعي الـAPI عند إلغاء كل القوائم صراحةً.
        var normalizedListIds = listIds is { Length: > 0 } ? listIds : null;
        var result = await _mediator.Send(
            new GetSalesMapPointsQuery
            {
                ProvinceId = provinceId,
                ListIds = normalizedListIds,
                Status = status
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementSalesMap.Read)]
    [HttpGet("orders/{orderId:int}")]
    public async Task<ActionResult<SalesMapOrderDetailDto>> OrderDetail([FromRoute] int orderId)
    {
        var result = await _mediator.Send(
            new GetSalesMapOrderDetailQuery { OrderId = orderId },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
