namespace GroomerManager.Infrastructure.Email;

public class ConfirmEmailSettings
{
    public string ActivationLink { get; set; }
    public int ActiveLinkTimeInHours { get; set; }
}