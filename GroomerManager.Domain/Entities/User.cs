using GroomerManager.Domain.Common;

namespace GroomerManager.Domain.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required UserInfo UserInfo { get; set; }
    public Guid RoleId { get; set; }
    public required Role Role { get; set; }
    public required RefreshToken RefreshToken { get; set; }
    public ICollection<UserSalon> UserSalons { get; private set; } = [];
}