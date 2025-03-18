using FluentValidation;
using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Domain.Entities;
using GroomerManager.Domain.Enums;
using GroomerManager.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Auth
{
    public abstract class CreateUserWithSalonCommand
    {
        public class CreateUserWithSalonRequest : IRequest<CreateUserWithSalonResponse>
        {
            public AccountDto Account { get; set; } = null!;
            public PersonalDto Personal { get; set; } = null!;
            public SalonDto Salon { get; set; } = null!;
        }

        public record AccountDto(string Email, string Password, string RepeatPassword);
        public record PersonalDto(string FirstName, string LastName, string Phone);
        public record SalonDto(string Name);

        public class CreateUserWithSalonResponse
        {
            public required string Message { get; set; }
        }

        public class Handler : BaseCommandHandler, IRequestHandler<CreateUserWithSalonRequest, CreateUserWithSalonResponse>
        {
            private readonly IGroomerManagerDbContext _dbContext;
            private readonly IPasswordManager _passwordManager;
            private readonly IDateTime _dateTime;
            private readonly IEmailSender _emailSender;
            private readonly IConfirmEmail _confirmEmail;
            private readonly IEmailSchema _emailSchema;

            public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IPasswordManager passwordManager, IDateTime dateTime, IEmailSender emailSender, IConfirmEmail confirmEmail, IEmailSchema emailSchema) 
                : base(dbContext, currentSalon)
            {
                _dbContext = dbContext;
                _passwordManager = passwordManager;
                _dateTime = dateTime;
                _emailSender = emailSender;
                _confirmEmail = confirmEmail;
                _emailSchema = emailSchema;
            }

            public async Task<CreateUserWithSalonResponse> Handle(CreateUserWithSalonRequest request, CancellationToken cancellationToken)
            {
                if (request.Account.Password != request.Account.RepeatPassword)
                {
                    throw new ErrorException("PasswordDontMatch");
                }
                
                var user = await _dbContext.Users.Include(x => x.UserInfo).FirstOrDefaultAsync(x => x.Email == request.Account.Email || x.UserInfo.Phone == request.Personal.Phone, cancellationToken);

                if (user != null)
                {
                    throw new ErrorException("UserAlreadyExists");
                }
                
                var ownerRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Owner", cancellationToken);

                if (ownerRole == null)
                {
                    throw new ErrorException("RoleNotFound");
                }
                
                var userDto = new Domain.Entities.User
                {
                    Email = request.Account.Email,
                    Password = _passwordManager.HashPassword(request.Account.Password),
                    UserInfo = null,
                    Role = ownerRole,
                    RefreshToken = null,
                };
                
                _dbContext.Users.Add(userDto);
                
                var userName = new UserName
                {
                    FirstName = request.Personal.FirstName,
                    LastName = request.Personal.LastName,
                };
                
                var userInfo = new UserInfo
                {
                    UserName = userName,
                    Phone = string.Concat(request.Personal.Phone.Where(c => !char.IsWhiteSpace(c))),
                    User = userDto
                };
                
                userDto.UserInfo = userInfo;
                
                _dbContext.UsersInfo.Add(userInfo);
                
                var salonDto = new Domain.Entities.Salon
                {
                    Name = request.Salon.Name,
                    CreatedBy = request.Account.Email,
                    IsDefault = true
                };
                
                
                salonDto.OwnerId = userDto.Id;
                
                _dbContext.Salons.Add(salonDto);
                
                var userSalon = new UserSalon
                {
                    User = userDto,
                    Salon = salonDto,
                    Role = "Owner"
                };
                
                _dbContext.UserSalons.Add(userSalon);
                
                var verificationToken = new EmailVerification
                {
                    UserId = userDto.Id,
                    Expires = _dateTime.Now.AddHours(_confirmEmail.GetActiveLinkTimeInHours())
                    
                };
    
                _dbContext.EmailVerifications.Add(verificationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var verificationLink = _confirmEmail.GetActivationLink(verificationToken.Id.ToString());

                var emailVerificationSchema = _emailSchema.GetEmailSchema(EmailSchemaEnum.CONFIRM_EMAIL, verificationLink);

                await _emailSender.SendEmailAsync(userDto.Email, emailVerificationSchema.Subject, emailVerificationSchema.Body);
                
                return new CreateUserWithSalonResponse
                {
                    Message = "AccountCreated"
                };
            }

            public class Validator : AbstractValidator<CreateUserWithSalonRequest>
            {
                public Validator()
                {
                    RuleFor(x => x.Account.Email)
                        .NotEmpty().WithMessage("EmailIsRequired")
                        .EmailAddress().WithMessage("InvalidEmailAddress");

                    RuleFor(x => x.Account.Password)
                        .NotEmpty().WithMessage("PasswordIsRequired")
                        .MinimumLength(8).WithMessage("PasswordTooShort")
                        .Matches("[A-Z]").WithMessage("PasswordMustContainUppercase")
                        .Matches("[a-z]").WithMessage("PasswordMustContainLowercase")
                        .Matches("[0-9]").WithMessage("PasswordMustContainNumber")
                        .Matches("[^a-zA-Z0-9]").WithMessage("PasswordMustContainSpecialCharacter");

                    RuleFor(x => x.Account.RepeatPassword)
                        .Equal(x => x.Account.Password).WithMessage("PasswordDontMatch");

                    RuleFor(x => x.Personal.FirstName)
                        .NotEmpty().WithMessage("FirstNameIsRequired")
                        .Length(1, 50).WithMessage("FirstNameIsTooLong");

                    RuleFor(x => x.Personal.LastName)
                        .NotEmpty().WithMessage("LastNameIsRequired")
                        .Length(1, 50).WithMessage("LastNameIsTooLong");

                    RuleFor(x => x.Personal.Phone)
                        .NotEmpty().WithMessage("PhoneIsRequired")
                        .Matches(@"^\+48\s?([0-9]{3})\s?([0-9]{3})\s?([0-9]{3})$")
                        .WithMessage("InvalidPhoneNumberFormat")
                        .MinimumLength(9).WithMessage("PhoneNumberMustContain9numbers");

                    RuleFor(x => x.Salon.Name)
                        .NotEmpty().WithMessage("SalonNameIsRequired");
                }
            }
        }
    }
}
