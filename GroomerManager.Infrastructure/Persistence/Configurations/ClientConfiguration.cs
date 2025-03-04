using GroomerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroomerManager.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasOne(c => c.Salon)
            .WithMany(s => s.Clients)
            .HasForeignKey(c => c.SalonId);
        
        builder.OwnsOne(c => c.ClientName);
    }
}