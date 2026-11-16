using Application.Common.Interfaces;
using Application.Users.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AdminArea;

/// <summary>هجرة تبويبات اللوحة — Development / قاعدة معزولة فقط. لا يُستدعى على الإنتاج.</summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/panel-tab-migration")]
public class PanelTabMigrationController : ApiController
{
    private readonly IApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PanelTabMigrationController(IApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HasPermission(Permissions.User.ManagePermissions)]
    [HttpPost("dry-run")]
    public async Task<ActionResult<PanelTabMigrationResult>> DryRun()
    {
        if (_env.IsProduction())
            return NotFound();

        var snapshotDir = Path.Combine(Path.GetTempPath(), "qalaat-panel-tab-migration");
        var result = await PanelTabAccountMigrator.RunAsync(_db, apply: false, snapshotDir, CancellationToken);
        return Ok(result);
    }

    [HasPermission(Permissions.User.ManagePermissions)]
    [HttpPost("apply")]
    public async Task<ActionResult<PanelTabMigrationResult>> Apply([FromQuery] bool confirmIsolated = false)
    {
        if (_env.IsProduction() || confirmIsolated == false)
            return NotFound();

        var snapshotDir = Path.Combine(Path.GetTempPath(), "qalaat-panel-tab-migration");
        var result = await PanelTabAccountMigrator.RunAsync(_db, apply: true, snapshotDir, CancellationToken);
        return Ok(result);
    }
}
