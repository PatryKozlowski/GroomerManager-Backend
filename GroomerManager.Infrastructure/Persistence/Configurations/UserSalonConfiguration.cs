using GroomerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroomerManager.Infrastructure.Persistence.Configurations;

public class UserSalonConfiguration: IEntityTypeConfiguration<UserSalon>
{
    public void Configure(EntityTypeBuilder<UserSalon> builder)
    {
        builder.HasKey(us => new { us.UserId, us.SalonId });

        builder.HasOne(us => us.User)
            .WithMany(u => u.UserSalons)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.Salon)
            .WithMany(s => s.UserSalons)
            .HasForeignKey(us => us.SalonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}