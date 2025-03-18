namespace GroomerManager.Infrastructure.Email;

public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPassword { get; set; }
    public string Sender { get; set; }
    public string SenderEmail { get; set; }
}