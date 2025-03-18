using GroomerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Common.Interfaces;

public interface IGroomerManagerDbContext
{
    DbSet<Domain.Entities.User> Users { get; set; }
    DbSet<Role> Roles { get; set; }
    DbSet<EmailVerification> EmailVerifications { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<Domain.Entities.Client> Clients { get; set; }
    DbSet<Domain.Entities.Salon> Salons { get; set; }
    DbSet<UserSalon> UserSalons { get; set; }
    DbSet<UserInfo> UsersInfo { get; set; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}