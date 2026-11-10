using Application.WrongSales.Commands;
using Application.WrongSales.Common;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/WrongSales")]
public class WrongSalesController : ApiController
{
    private readonly IMediator _mediator;

    public WrongSalesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// تقييم أول تحصيل للمندوب — يولّد WrongSaleNotice عند ≤6.
    /// الكاتب من الجلسة؛ ListId يجب أن يكون مكلَّفاً للمندوب.
    /// </summary>
    [HasPermission(Permissions.InstallmentPayment.Create)]
    [HttpPost("first-collection-eval")]
    public async Task<ActionResult<MandobFirstCollectionEvalResultDto>> FirstCollectionEval([FromBody] Body body)
        => (await _mediator.Send(new ReportMandobFirstCollectionWrongSaleCommand
        {
            CustomerId = body.CustomerId,
            ListId = body.ListId,
            Rating = body.Rating,
            Note = body.Note,
            ShopStockEstimatedValue = body.ShopStockEstimatedValue,
            EstimatedDailyRevenue = body.EstimatedDailyRevenue,
            SaleAmount = body.SaleAmount
        }, CancellationToken)).ToHttpResponse();

    public sealed class Body
    {
        public int CustomerId { get; set; }
        public int ListId { get; set; }
        public int Rating { get; set; }
        public string? Note { get; set; }
        public decimal ShopStockEstimatedValue { get; set; }
        public decimal EstimatedDailyRevenue { get; set; }
        public decimal? SaleAmount { get; set; }
    }
}
