namespace GroomerManager.Infrastructure.Auth;

public class JwtOptions
{
    public required string Secret { get; set; }
    public required string RefreshTokenSecret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int Expires { get; set; }
}