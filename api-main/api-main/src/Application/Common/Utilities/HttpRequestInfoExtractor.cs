using System.Text.RegularExpressions;
using Domain.Folder;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Utilities;

public partial class HttpRequestInfoExtractor
{
    /// <summary>
    /// Extracts session information from HTTP request
    /// </summary>
    public static HttpRequestInfo Extract(HttpRequest request)
    {
        var userAgent = GetUserAgent(request);

        return new HttpRequestInfo
        {
            IpAddress = GetClientIpAddress(request),
            UserAgent = userAgent,
            Browser = GetBrowser(userAgent),
            OperatingSystem = GetOperatingSystem(userAgent),
            DeviceType = GetDeviceType(userAgent)
        };
    }

    /// <summary>
    /// Gets the client's IP address, considering proxies and load balancers
    /// </summary>
    private static string? GetClientIpAddress(HttpRequest request)
    {
        // proxies/load balancers
        var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var firstIp = forwardedFor.Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(firstIp))
            {
                return firstIp;
            }
        }

        var realIp = request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Cloudflare
        var cfIp = request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfIp))
        {
            return cfIp;
        }

        return request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Gets the User-Agent string from request headers
    /// </summary>
    private static string? GetUserAgent(HttpRequest request)
    {
        return request.Headers.UserAgent.FirstOrDefault();
    }

    /// <summary>
    /// Parses browser name from User-Agent string
    /// </summary>
    private static string? GetBrowser(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return null;

        var ua = userAgent.ToLower();

        if (ua.Contains("edg/") || ua.Contains("edga/") || ua.Contains("edgios/"))
            return "Edge";
        if (ua.Contains("opr/") || ua.Contains("opera"))
            return "Opera";
        if (ua.Contains("chrome/") || ua.Contains("crios/"))
            return "Chrome";
        if (ua.Contains("firefox/") || ua.Contains("fxios/"))
            return "Firefox";
        if (ua.Contains("safari/") && !ua.Contains("chrome"))
            return "Safari";
        if (ua.Contains("msie") || ua.Contains("trident/"))
            return "Internet Explorer";
        if (ua.Contains("brave"))
            return "Brave";
        if (ua.Contains("vivaldi"))
            return "Vivaldi";
        if (ua.Contains("samsung"))
            return "Samsung Internet";
        if (ua.Contains("ucbrowser"))
            return "UC Browser";

        return null;
    }

    /// <summary>
    /// Parses operating system from User-Agent string
    /// </summary>
    private static string? GetOperatingSystem(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return null;

        var ua = userAgent.ToLower();

        // Windows
        if (ua.Contains("windows nt 10.0"))
            return "Windows 10/11";
        if (ua.Contains("windows nt 6.3"))
            return "Windows 8.1";
        if (ua.Contains("windows nt 6.2"))
            return "Windows 8";
        if (ua.Contains("windows nt 6.1"))
            return "Windows 7";
        if (ua.Contains("windows"))
            return "Windows";

        // macOS
        if (ua.Contains("mac os x"))
        {
            var match = MacOs().Match(ua);
            if (match.Success)
            {
                return $"macOS {match.Groups[1].Value}.{match.Groups[2].Value}";
            }

            return "macOS";
        }

        // iOS
        if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod"))
        {
            if (ua.Contains("ipad"))
                return "iOS (iPad)";
            return "iOS";
        }

        // Android
        if (ua.Contains("android"))
        {
            var match = Android().Match(ua);
            if (match.Success)
            {
                return $"Android {match.Groups[1].Value}";
            }

            return "Android";
        }

        // Linux
        if (ua.Contains("linux"))
            return "Linux";

        // Chrome OS
        if (ua.Contains("cros"))
            return "Chrome OS";

        return null;
    }

    /// <summary>
    /// Determines device type from User-Agent string
    /// </summary>
    private static DeviceType GetDeviceType(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return DeviceType.Unknown;

        var ua = userAgent.ToLower();

        // bots/crawlers
        if (ua.Contains("bot") || ua.Contains("crawler") || ua.Contains("spider") ||
            ua.Contains("slurp") || ua.Contains("googlebot") || ua.Contains("bingbot") ||
            ua.Contains("facebookexternalhit") || ua.Contains("linkedinbot"))
        {
            return DeviceType.Bot;
        }

        // tablets
        if (ua.Contains("ipad") ||
            ua.Contains("tablet") ||
            ua.Contains("kindle") ||
            (ua.Contains("android") && !ua.Contains("mobile")))
        {
            return DeviceType.Tablet;
        }

        // mobile
        if (ua.Contains("mobile") || ua.Contains("iphone") || ua.Contains("ipod") ||
            ua.Contains("android") || ua.Contains("blackberry") ||
            ua.Contains("windows phone") || ua.Contains("webos") ||
            ua.Contains("opera mini") || ua.Contains("iemobile"))
        {
            return DeviceType.Mobile;
        }

        return DeviceType.Desktop;
    }
    
    public class HttpRequestInfo
    {
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DeviceType DeviceType { get; set; }
        public string? Browser { get; set; }
        public string? OperatingSystem { get; set; }
    }

    [GeneratedRegex(@"mac os x (\d+)[_.](\d+)")]
    private static partial Regex MacOs();
    [GeneratedRegex(@"android (\d+(?:\.\d+)?)")]
    private static partial Regex Android();
}