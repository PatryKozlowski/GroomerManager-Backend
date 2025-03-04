namespace GroomerManager.Application.Common.Interfaces;

public interface IAuthenticationDataProvider
{
    Guid? GetUserId();
    string? GetUserEmail();
    string? GetUserRefreshTokenFromCookie();
}