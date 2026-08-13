namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
public class AppController : ApiController
{
    private const int LatestAppBuildNumber = 5;
    
    [HttpGet("checkVersion")]
    public ActionResult<AppVersionResponse> CheckVersion([FromQuery] int buildNumber)
    {
        var response = new AppVersionResponse
        {
            ShouldUpdate = buildNumber < LatestAppBuildNumber,
            IsForce = true,
            Url = ""
        };
        return Ok(response);
    }
}

public record AppVersionResponse
{
    public bool ShouldUpdate { get; set; }
    public bool IsForce { get; set; }
    public required string Url { get; set; }
}