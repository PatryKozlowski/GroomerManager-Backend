namespace GroomerManager.Application.Common.Interfaces;

public interface IPasswordManager
{
    string HashPassword(string password);
    bool VerifyPassword(string hash, string password);
}