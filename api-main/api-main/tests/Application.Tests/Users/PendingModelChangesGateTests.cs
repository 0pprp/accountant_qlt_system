using Application.Common.Utilities;
using Microsoft.Extensions.Configuration;

namespace Application.Tests.Users;

public class PendingModelChangesGateTests
{
    [Fact]
    public void Default_settings_do_not_ignore_pending_model_changes()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var cs = "Host=localhost;Port=5432;Database=qalaat_phase02_verify;Username=postgres;Password=x";

        Assert.False(PendingModelChangesGate.ShouldIgnorePendingModelChanges(
            config, "Development", cs));
    }

    [Fact]
    public void Production_environment_never_ignores_even_with_flag()
    {
        var config = BuildConfig(allow: true);
        var cs = "Host=localhost;Port=5432;Database=qalaat_phase02_verify;Username=postgres;Password=x";

        Assert.False(PendingModelChangesGate.ShouldIgnorePendingModelChanges(
            config, "Production", cs));
    }

    [Fact]
    public void Database_name_alone_is_not_enough_when_host_is_remote()
    {
        var config = BuildConfig(allow: true);
        var cs = "Host=77.238.108.116;Port=5432;Database=qalaat_phase02_verify;Username=postgres;Password=x";

        Assert.False(PendingModelChangesGate.ShouldIgnorePendingModelChanges(
            config, "Development", cs));
        Assert.False(PendingModelChangesGate.IsDedicatedLocalVerifyConnection(cs));
    }

    [Fact]
    public void Local_host_with_wrong_database_is_rejected()
    {
        var config = BuildConfig(allow: true);
        var cs = "Host=127.0.0.1;Port=5432;Database=QalaatAlDamanDb;Username=postgres;Password=x";

        Assert.False(PendingModelChangesGate.ShouldIgnorePendingModelChanges(
            config, "Development", cs));
    }

    [Fact]
    public void Development_flag_local_loopback_and_allowed_database_enables_ignore()
    {
        var config = BuildConfig(allow: true);
        var cs = "Host=127.0.0.1;Port=5432;Database=qalaat_phase02_verify;Username=postgres;Password=x";

        Assert.True(PendingModelChangesGate.ShouldIgnorePendingModelChanges(
            config, "Development", cs));
    }

    private static IConfiguration BuildConfig(bool allow) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{PendingModelChangesGate.SectionName}:{PendingModelChangesGate.AllowPendingModelChangesKey}"] =
                    allow ? "true" : "false",
                [$"{PendingModelChangesGate.SectionName}:{PendingModelChangesGate.AllowedDatabaseKey}"] =
                    PendingModelChangesGate.DefaultAllowedDatabase,
            })
            .Build();
}
