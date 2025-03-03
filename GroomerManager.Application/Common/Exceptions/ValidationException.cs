namespace GroomerManager.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public List<FieldError> Errors { get; set; } = [];
    
    public class FieldError
    {
        public required string FieldName { get; set; }
        public required string Error { get; set; }
    }
}