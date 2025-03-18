namespace GroomerManager.Application.Common.Interfaces;

public interface IConfirmEmail
{
    string GetActivationLink(string token);
    int GetActiveLinkTimeInHours();
}