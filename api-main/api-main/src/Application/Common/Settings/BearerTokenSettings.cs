namespace Application.Common.Settings;

public class BearerTokenSettings
{
    public string SecretKey { set; get; } = string.Empty;
    public string Issuer { set; get; } = string.Empty;
    public string Audience { set; get; } = string.Empty;
    public int AccessTokenExpirationMinutes { set; get; }
    public int RefreshTokenExpirationMinutes { set; get; }
}