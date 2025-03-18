using System.Net;
using System.Net.Mail;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Infrastructure.Email;
using Microsoft.Extensions.Options;

namespace GroomerManager.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }
    
    public Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SmtpClient
        {
            EnableSsl = _emailSettings.EnableSsl,
            Host = _emailSettings.SmtpServer,
            Port = _emailSettings.SmtpPort,
            TargetName = _emailSettings.Sender,
            Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword)
        };

        return client.SendMailAsync(new MailMessage(from: _emailSettings.SenderEmail, to: email, subject, message));
    }
}