using GroomerManager.Domain.Common;

namespace GroomerManager.Domain.Entities;

public class EmailVerification : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTimeOffset Expires { get; set; }
    public User User { get; set; }
}