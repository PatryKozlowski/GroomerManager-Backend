using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Auth;

public abstract class ConfirmationUserEmailCommand
{
    public class ConfirmationUserEmailRequest : IRequest<ConfirmationUserEmailResponse>
    {
        public Guid Token { get; set; }
    }
    
    public class ConfirmationUserEmailResponse
    {
        public required string Message { get; set; }
    }
    
    public class Handler : BaseCommandHandler, IRequestHandler<ConfirmationUserEmailRequest, ConfirmationUserEmailResponse>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly IDateTime _dateTime;

        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IDateTime dateTime) : base(dbContext, currentSalon)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        public async Task<ConfirmationUserEmailResponse> Handle(ConfirmationUserEmailRequest request, CancellationToken cancellationToken)
        {
            var token = await _dbContext.EmailVerifications
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == request.Token, cancellationToken);

            if (token == null)
            {
                throw new ErrorException("ConfirmTokenDontExist");
            }

            if (token.Expires < _dateTime.Now)
            {
                throw new ErrorException("TokenExpired");
            }

            if (token.User.IsEmailConfirmed)
            {
                throw new ErrorException("EmailAlreadyConfirmed");
            }
            
            token.User.IsEmailConfirmed = true;
            
            _dbContext.Users.Update(token.User);

            _dbContext.EmailVerifications.Remove(token);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ConfirmationUserEmailResponse
            {
                Message = "EmailSuccessConfirmed"
            };
        }
    }
}