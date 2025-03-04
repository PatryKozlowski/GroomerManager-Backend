using GroomerManager.Domain.Common;

namespace GroomerManager.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public required string Token { get; set; }
    public required User User { get; set; }
}