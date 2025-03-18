using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.User;

public abstract class GetUserInfoCommand
{
    public class GetUserInfoRequest : IRequest<GetUserInfoResponse>
    {
    }

    public class GetUserInfoResponse
    {
        public required Guid Id { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string FullName { get; set; }
    }
    
    public class Handler : BaseCommandHandler, IRequestHandler<GetUserInfoRequest, GetUserInfoResponse>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly IAuthenticationDataProvider _authenticationData;

        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IAuthenticationDataProvider authenticationData) : base(dbContext, currentSalon)
        {
            _dbContext = dbContext;
            _authenticationData = authenticationData;
        }

        public async Task<GetUserInfoResponse> Handle(GetUserInfoRequest request, CancellationToken cancellationToken)
        {
            var userId = _authenticationData.GetUserId();
            
            if (userId == null)
            {
                throw new UnauthorizedException();
            }

            var user = await _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.UserInfo)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
            if (user == null)
            {
                throw new ErrorException("UserNotFound");
            }

            if (user.UserInfo == null)
            {
                throw new ErrorException("UserInfoNotFound");
            }
            
            return new GetUserInfoResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.Name,
                FullName = user.UserInfo.UserName.ToString()
            };
        }
    }
}