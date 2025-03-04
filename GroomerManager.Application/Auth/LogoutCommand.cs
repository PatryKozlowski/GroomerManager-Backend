using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using MediatR;

namespace GroomerManager.Application.Auth;

public abstract class LogoutCommand
{
    public class LogoutRequest : IRequest<LogoutResponse>
    {
    }

    public class LogoutResponse
    {
        public required string Message { get; set; }
    }
    
    public class Handler : BaseCommandHandler, IRequestHandler<LogoutRequest, LogoutResponse>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly IAuthenticationDataProvider _authenticationData;

        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IAuthenticationDataProvider authenticationData) : base(dbContext, currentSalon)
        {
            _dbContext = dbContext;
            _authenticationData = authenticationData;
        }

        public async Task<LogoutResponse> Handle(LogoutRequest request, CancellationToken cancellationToken)
        {
            var userId = _authenticationData.GetUserId();

            if (userId == null)
            {
                throw new UnauthorizedException();
            }
            
            return new LogoutResponse
            {
                Message = "LogoutSuccessful"
            };
        }
    }
}