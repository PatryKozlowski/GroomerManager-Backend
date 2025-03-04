using GroomerManager.Domain.Common;

namespace GroomerManager.Domain.Entities;

public class UserSalon : BaseEntity
{
    public Guid UserId { get; set; }
    public required User User { get; set; }
    public Guid SalonId { get; set; }
    public required Salon Salon { get; set; } 
    public required string Role { get; set; }
}