using GroomerManager.Domain.Common;
using GroomerManager.Domain.ValueObjects;

namespace GroomerManager.Domain.Entities;

public class Client : BaseEntity
{
    public required ClientName ClientName { get; set; }

    public required string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid SalonId { get; set; }
    public  required Salon Salon { get; set; }
}