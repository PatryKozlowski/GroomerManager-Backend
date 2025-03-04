using GroomerManager.Domain.Common;
using GroomerManager.Domain.ValueObjects;

namespace GroomerManager.Domain.Entities;

public class UserInfo : BaseEntity
{
    public required UserName UserName { get; set; }
    public Guid UserId { get; set; }
    public required User User { get; set; }
}