using GroomerManager.Domain.Entities;

namespace GroomerManager.Application.Common.Interfaces;

public interface IJwtManager
{
    bool ValidateToken(string token, bool isRefreshToken);
    string GenerateUserToken(Guid userId, string email, Role role, bool isRefreshToken);
    string? GetClaim(string token, string claimType);
    long GetTokenExpiration(string token);
}