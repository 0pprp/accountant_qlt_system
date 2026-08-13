namespace Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[Controller]")]
public abstract class ApiController : ControllerBase
{
    protected CancellationToken CancellationToken => HttpContext.RequestAborted;
}