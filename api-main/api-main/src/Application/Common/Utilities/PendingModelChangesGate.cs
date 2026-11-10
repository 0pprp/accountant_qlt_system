using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Application.Common.Utilities;

/// <summary>
/// يقيّد تجاهل PendingModelChangesWarning بتجربة محلية معزولة فقط.
/// الافتراضي: غير مفعّل.
/// </summary>
public static class PendingModelChangesGate
{
    public const string SectionName = "Phase02LocalVerify";
    public const string AllowPendingModelChangesKey = "AllowPendingModelChanges";
    public const string AllowedDatabaseKey = "AllowedDatabase";
    public const string DefaultAllowedDatabase = "qalaat_phase02_verify";

    public static bool ShouldIgnorePendingModelChanges(
        IConfiguration configuration,
        string? environmentName,
        string? connectionString)
    {
        if (!IsDevelopment(environmentName))
            return false;

        if (configuration.GetValue($"{SectionName}:{AllowPendingModelChangesKey}", false) == false)
            return false;

        return IsDedicatedLocalVerifyConnection(
            connectionString,
            configuration.GetValue($"{SectionName}:{AllowedDatabaseKey}", DefaultAllowedDatabase));
    }

    public static bool IsDedicatedLocalVerifyConnection(
        string? connectionString,
        string? allowedDatabase = DefaultAllowedDatabase)
    {
        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(allowedDatabase))
            return false;

        try
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
            var host = GetValue(builder, "Host") ?? GetValue(builder, "Server");
            var database = GetValue(builder, "Database");

            if (IsLocalLoopbackHost(host) == false)
                return false;

            return string.Equals(database, allowedDatabase, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static bool IsLocalLoopbackHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        var normalized = host.Trim().TrimStart('[').TrimEnd(']');
        return normalized.Equals("localhost", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("127.0.0.1", StringComparison.Ordinal)
               || normalized.Equals("::1", StringComparison.Ordinal);
    }

    private static bool IsDevelopment(string? environmentName) =>
        string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase);

    private static string? GetValue(DbConnectionStringBuilder builder, string key)
    {
        if (builder.TryGetValue(key, out var value) == false || value is null)
            return null;

        var text = value.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
