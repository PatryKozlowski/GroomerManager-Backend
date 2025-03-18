using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Domain.Enums;
using GroomerManager.Infrastructure.Email;

namespace GroomerManager.Infrastructure.Services;

public class EmailSchema : IEmailSchema
{
    public (string Subject, string Body) GetEmailSchema(EmailSchemaEnum schema, string? stringParams = null)
    {
        return schema switch
        {
            EmailSchemaEnum.CONFIRM_EMAIL => EmailSenderSchema.ConfirmationEmailSchema(stringParams!),
            _ => throw new ArgumentOutOfRangeException(nameof(schema), schema, "This email schema not exist")
        };
    }
}