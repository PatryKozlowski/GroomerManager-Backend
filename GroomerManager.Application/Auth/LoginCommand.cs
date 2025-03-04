using FluentValidation;
using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Auth;

public abstract class LoginCommand
{
    public class LoginRequest : IRequest<LoginResponse>
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponse
    {
        public required string Token { get; set; }
        public required long TokenExpired { get; set; }
        public required string RefreshToken { get; set; }
    }
    
    public class Handler : BaseCommandHandler, IRequestHandler<LoginRequest, LoginResponse>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly ICurrentSalonProvider _currentSalon;
        private readonly IPasswordManager _passwordManager;
        private readonly IJwtManager _jwtManager;
        
        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IPasswordManager passwordManager, IJwtManager jwtManager) : base(dbContext, currentSalon)
        {
            _dbContext = dbContext;
            _currentSalon = currentSalon;
            _passwordManager = passwordManager;
            _jwtManager = jwtManager;
        }

        public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
            {
                throw new ErrorException("InvalidEmailOrPassword");
            }

            if (_passwordManager.VerifyPassword(user.Password, request.Password))
            {
                var token = _jwtManager.GenerateUserToken(user.Id, user.Email, user.Role, false);
                var refreshToken = _jwtManager.GenerateUserToken(user.Id, user.Email, user.Role, true);
                
                var existingToken = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.UserId == user.Id, cancellationToken);

                if (existingToken != null)
                {
                    existingToken.Token = refreshToken;
                }
                else
                {
                    var newToken = new RefreshToken
                    {
                        UserId = user.Id,
                        Token = refreshToken,
                        User = user
                    };
                    _dbContext.RefreshTokens.Add(newToken);
                }
                
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return new LoginResponse
                {
                    Token = token,
                    TokenExpired = _jwtManager.GetTokenExpiration(token),
                    RefreshToken = refreshToken
                };
            }
            
            throw new ErrorException("InvalidEmailOrPassword");
        }
    }

    public class Validator : AbstractValidator<LoginRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("EmailIsRequired")
                .EmailAddress().WithMessage("InvalidEmailAddress");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("PasswordIsRequired")
                .MinimumLength(8).WithMessage("PasswordTooShort")
                .Matches("[A-Z]").WithMessage("PasswordMustContainUppercase")
                .Matches("[a-z]").WithMessage("PasswordMustContainLowercase")
                .Matches("[0-9]").WithMessage("PasswordMustContainNumber")
                .Matches("[^a-zA-Z0-9]").WithMessage("PasswordMustContainSpecialCharacter");
        }
    }
}