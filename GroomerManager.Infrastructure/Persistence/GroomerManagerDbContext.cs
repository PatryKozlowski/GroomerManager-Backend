using System.Linq.Expressions;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Domain.Common;
using GroomerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Infrastructure.Persistence;

public class GroomerManagerDbContext : DbContext, IGroomerManagerDbContext
{
    private readonly IDateTime _dateTime;
    private readonly IAuthenticationDataProvider _authenticationData;
    
    public GroomerManagerDbContext(DbContextOptions<GroomerManagerDbContext> options, IDateTime dateTime, IAuthenticationDataProvider authenticationData)
        : base(options)
    {
        _dateTime = dateTime;
        _authenticationData = authenticationData;
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Salon> Salons { get; set; }
    public DbSet<UserSalon> UserSalons { get; set; }
    public DbSet<UserInfo> UsersInfo { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.StatusId));
                var condition = Expression.Equal(property, Expression.Constant(1));
                var lambda = Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GroomerManagerDbContext).Assembly);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userEmail = _authenticationData.GetUserEmail() ?? "System";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Created = _dateTime.Now;
                    entry.Entity.CreatedBy = userEmail;
                    entry.Entity.StatusId = 1;
                    break;

                case EntityState.Modified:
                    entry.Entity.Modified = _dateTime.Now;
                    entry.Entity.ModifiedBy = userEmail;
                    break;

                case EntityState.Deleted:
                    entry.Entity.Modified = _dateTime.Now;
                    entry.Entity.Inactivated = _dateTime.Now;
                    entry.Entity.InactivatedBy = userEmail;
                    entry.Entity.StatusId = 0;
                    entry.State = EntityState.Modified;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}