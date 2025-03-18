using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Infrastructure.Email;
using Microsoft.Extensions.Options;

namespace GroomerManager.Infrastructure.Services;

public class ConfirmEmail : IConfirmEmail
{
    private readonly ConfirmEmailSettings _confirmEmail;

    public ConfirmEmail(IOptions<ConfirmEmailSettings> confirmEmail)
    {
        _confirmEmail = confirmEmail.Value;
    }

    public string GetActivationLink(string token)
    {
        var activationLink = _confirmEmail.ActivationLink;

        return activationLink + $"?token={token}";
    }

    public int GetActiveLinkTimeInHours()
    {
        return _confirmEmail.ActiveLinkTimeInHours;
    }
}