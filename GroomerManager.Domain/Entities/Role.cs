using GroomerManager.Domain.Common;

namespace GroomerManager.Domain.Entities;

public class Role : BaseEntity
{
    public required string Name { get; set; }
    public required User User { get; set; }
}