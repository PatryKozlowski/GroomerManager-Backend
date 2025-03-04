using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Infrastructure.Auth;

namespace GroomerManager.API.Auth;

public class JwtAuthenticationDataProvider : IAuthenticationDataProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtManager _jwtManager;

    public JwtAuthenticationDataProvider(IHttpContextAccessor httpContextAccessor, IJwtManager jwtManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtManager = jwtManager;
    }
    
    public Guid? GetUserId()
    {
        var userIdString = GetClaimValue(JwtManager.USER_ID_CLAIM);

        if (Guid.TryParse(userIdString, out var result))
        {
            return result;
        }

        return null;
    }

    public string? GetUserEmail()
    {
        var userEmail = GetClaimValue(JwtManager.USER_EMAIL_CLAIM);

        return userEmail ?? null;
    }

    public string? GetUserRefreshTokenFromCookie()
    {
        return GetTokenFromCookie(true) ?? null;
    }
    
    private string? GetTokenFromHeader()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return null;
        }

        var splited = authorizationHeader.Split(' ');

        if (splited.Length > 1 && splited[0] == "Bearer")
        {
            return splited[1];
        }

        return null;
    }
    
    private string? GetTokenFromCookie(bool isRefreshToken = false)
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[isRefreshToken ? CookieSettings.REFRESH_COKIE_NAME : CookieSettings.COOKIE_NAME ];
    }
    
    private string? GetClaimValue(string claimType)
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
        {
            token = GetTokenFromCookie();
        }
        
        if (!string.IsNullOrWhiteSpace(token) && _jwtManager.ValidateToken(token, false))
        {
            return _jwtManager.GetClaim(token, claimType);
        }

        return null;
    }
}