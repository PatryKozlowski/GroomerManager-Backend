using FluentValidation;
using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Interfaces;
using MediatR;

namespace GroomerManager.Application.Auth;

public abstract class GeneratePasswordCommand
{
    public class GeneratePasswordRequest : IRequest<GeneratePasswordResponse>
    {
        public required string Password { get; set; }
    }

    public class GeneratePasswordResponse
    {
        public required string Password { get; set; }
    }
    
    public class Handler : BaseCommandHandler, IRequestHandler<GeneratePasswordRequest, GeneratePasswordResponse>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly IPasswordManager _passwordManager;
        
        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IPasswordManager passwordManager) : base(dbContext, currentSalon)
        {
            _dbContext = dbContext;
            _passwordManager = passwordManager;
        }

        public async Task<GeneratePasswordResponse> Handle(GeneratePasswordRequest request, CancellationToken cancellationToken)
        {
            var hashedPassword = _passwordManager.HashPassword(request.Password);

            return new GeneratePasswordResponse()
            {
                Password = hashedPassword
            };
        }
    }
    
    public class Validator : AbstractValidator<GeneratePasswordRequest>
    {
        public Validator()
        {
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