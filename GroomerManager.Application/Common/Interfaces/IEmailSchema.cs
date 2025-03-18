using GroomerManager.Domain.Enums;

namespace GroomerManager.Application.Common.Interfaces;

public interface IEmailSchema
{
    (string Subject, string Body) GetEmailSchema(EmailSchemaEnum schema, string? stringParams = null);
}