namespace GroomerManager.Infrastructure.Email;

public class EmailSenderSchema
{
    public string Subject { get; set; }
    public string Body { get; set; }

    public static (string Subject, string Body) ConfirmationEmailSchema(string confirmationLink)
    {
        return (
            Subject: "Potwierdzenie konta",
            Body: $"Kliknij tutaj, aby potwierdzić swoje konto: <a href=\"{confirmationLink}\">Potwierdź Email</a>"
        );
    }
}