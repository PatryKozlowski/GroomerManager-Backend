using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Auth;

public abstract class RefreshTokenCommand
{
    public class RefreshTokenRequest : IRequest<RefreshTokenResponse>
    {
    }

    public class RefreshTokenResponse
    {
        public required string Token { get; set; }
        public required long TokenExpired { get; set; }
        public required string RefreshToken { get; set; }
    }
    
    public class Handler : BaseCommandHandler, IRequestHandler<RefreshTokenRequest, RefreshTokenResponse>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly IAuthenticationDataProvider _authenticationData;
        private readonly IJwtManager _jwtManager;
        private readonly IDateTime _dateTime;
        
        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IAuthenticationDataProvider authenticationData, IJwtManager jwtManager, IDateTime dateTime) : base(dbContext, currentSalon)
        {
            _dbContext = dbContext;
            _authenticationData = authenticationData;
            _jwtManager = jwtManager;
            _dateTime = dateTime;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var refreshTokenFromCookie = _authenticationData.GetUserRefreshTokenFromCookie();

            if (refreshTokenFromCookie == null)
            {
                throw new UnauthorizedException();
            }
            
            var user = await _dbContext.RefreshTokens
                .Where(n => n.Token == refreshTokenFromCookie)
                .Include(n => n.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(cancellationToken);
            

            if (user == null)
            {
                throw new UnauthorizedException();
            }
            
            var token = _jwtManager.GenerateUserToken(user.User.Id, user.User.Email, user.User.Role, false);
            var refreshToken = _jwtManager.GenerateUserToken(user.User.Id, user.User.Email, user.User.Role, true);
            
            var existingToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == user.User.Id, cancellationToken);

            if (existingToken != null)
            {
                existingToken.Token = refreshToken;
            }
            else
            {
                var newToken = new RefreshToken
                {
                    UserId = user.User.Id,
                    Token = refreshToken,
                    User = user.User
                };
                _dbContext.RefreshTokens.Add(newToken);
            }
  
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new RefreshTokenResponse
            {
                Token = token,
                TokenExpired = _jwtManager.GetTokenExpiration(token),
                RefreshToken = refreshToken
            };
        }
    }
}