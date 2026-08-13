using Application.Common.Models.KeysetPagination;
using Application.Customers.Commands.App.Create;
using Application.Customers.Commands.App.Update;
using Application.Customers.Queries.App.GetById;
using Application.Customers.Queries.App.GetCustomerInstallmentPayments;
using Application.Customers.Queries.App.GetCustomerOrders;
using Application.Customers.Queries.App.GetFinancialOverview;
using Application.Customers.Queries.App.GetPaginated;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class CustomersController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CustomersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetCustomerByIdResponse>> GetById([FromRoute] int id)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("{id:int}/orders")]
    public async Task<ActionResult<List<GetCustomerOrdersResponse>>> GetCustomerOrders([FromRoute] int id)
    {
        var query = new GetCustomerOrdersQuery
        {
            CustomerId = id,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("{id:int}/financial-overview")]
    public async Task<ActionResult<GetCustomerFinancialOverviewResponse>> GetCustomerFinancialOverview([FromRoute] int id)
    {
        var query = new GetCustomerFinancialOverviewQuery
        {
            CustomerId = id,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet("{id:int}/installment-payments")]
    public async Task<ActionResult<List<GetCustomerInstallmentPaymentsResponse>>> GetCustomerInstallmentPayments([FromRoute] int id)
    {
        var query = new GetCustomerInstallmentPaymentsQuery
        {
            CustomerId = id,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetPaginatedCustomersResponse>>> GetPaginated(
        [FromQuery] KeysetPagination<DateTimeOffset?> keysetPagination,
        [FromQuery] GetPaginatedCustomersFilter filter,
        [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedCustomersQuery
        {
            Pagination = keysetPagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Customer.Create)]
    [HttpPost]
    public async Task<ActionResult<CreateCustomerResponse>> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Customer.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateCustomerDto dto)
    {
        var command = new UpdateCustomerCommand
        {
            Id = id,
            FullName = dto.FullName,
            MotherName = dto.MotherName,
            NationalCode = dto.NationalCode,
            BirthDate = dto.BirthDate,
            PhoneNumber = dto.PhoneNumber,
            WhatsAppPhoneNumber = dto.WhatsAppPhoneNumber,
            Business = dto.Business
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
}