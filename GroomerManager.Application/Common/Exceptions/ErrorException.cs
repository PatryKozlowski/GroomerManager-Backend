namespace GroomerManager.Application.Common.Exceptions;

public class ErrorException(string message) : Exception(message)
{
}