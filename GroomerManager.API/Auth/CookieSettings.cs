namespace GroomerManager.API.Auth;

public class CookieSettings
{
    public const string COOKIE_NAME = "GROOMER-AUTH";
    public const string REFRESH_COKIE_NAME = "GROOMER-AUTH-REFRESH";
    public bool Secure { get; set; } = true;
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
}