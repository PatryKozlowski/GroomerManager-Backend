using FluentValidation;
using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Domain.Entities;
using GroomerManager.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Auth;

public abstract class NewConfirmEmailTokenCommand
{
    public class NewConfirmEmailTokenRequest : IRequest<NewConfirmEmailTokenResponse>
    {
        public required string Email { get; set; }
    }

    public class NewConfirmEmailTokenResponse
    {
        public required string Message { get; set; }
    }

    public class Handler : BaseCommandHandler,
        IRequestHandler<NewConfirmEmailTokenRequest, NewConfirmEmailTokenResponse>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly IDateTime _dateTime;
        private readonly IEmailSender _emailSender;
        private readonly IConfirmEmail _confirmEmail;
        private readonly IEmailSchema _emailSchema;

        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IDateTime dateTime,
            IEmailSender emailSender, IConfirmEmail confirmEmail, IEmailSchema emailSchema) : base(dbContext,
            currentSalon)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _emailSender = emailSender;
            _emailSchema = emailSchema;
            _confirmEmail = confirmEmail;
        }

        public async Task<NewConfirmEmailTokenResponse> Handle(NewConfirmEmailTokenRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
            {
                throw new ErrorException("UserDontExist");
            }

            if (user.IsEmailConfirmed)
            {
                throw new ErrorException("EmailAlreadyConfirmed");
            }

            var verificationToken = new EmailVerification
            {
                UserId = user.Id,
                Expires = _dateTime.Now.AddHours(_confirmEmail.GetActiveLinkTimeInHours())

            };

            _dbContext.EmailVerifications.Add(verificationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var verificationLink = _confirmEmail.GetActivationLink(verificationToken.Id.ToString());

            var emailVerificationSchema = _emailSchema.GetEmailSchema(EmailSchemaEnum.CONFIRM_EMAIL, verificationLink);

            await _emailSender.SendEmailAsync(request.Email, emailVerificationSchema.Subject,
                emailVerificationSchema.Body);

            return new NewConfirmEmailTokenResponse()
            {
                Message = "Success"
            };
        }
    }

    public class Validator : AbstractValidator<NewConfirmEmailTokenRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("EmailIsRequired")
                .EmailAddress().WithMessage("InvalidEmailAddress");
        }
    }
}   