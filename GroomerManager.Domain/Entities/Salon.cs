using GroomerManager.Domain.Common;

namespace GroomerManager.Domain.Entities;

public class Salon : BaseEntity
{
    public required string Name { get; set; }
    public string? LogoPath { get; set; }
    public  Guid? LogoId { get; set; }
    public required bool IsDefault { get; set; }
    public string? Address { get; set; }
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public ICollection<UserSalon> UserSalons { get; private set; } = new List<UserSalon>();
    public ICollection<Client> Clients { get; private set; } = new List<Client>();
} 