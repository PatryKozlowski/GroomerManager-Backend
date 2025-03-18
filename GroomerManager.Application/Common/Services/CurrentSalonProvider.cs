using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Common.Services;

public class CurrentSalonProvider : ICurrentSalonProvider
{
    private readonly IAuthenticationDataProvider _authenticationData;
    private readonly IGroomerManagerDbContext _dbContext;

    public CurrentSalonProvider(IAuthenticationDataProvider authenticationData, IGroomerManagerDbContext dbContext)
    {
        _authenticationData = authenticationData;
        _dbContext = dbContext;
    }
    
    public async Task<List<Guid>?> GetSalonId()
    {
        var userId = _authenticationData.GetUserId();

        if (userId != null)
        {
            return await _dbContext.UserSalons
                .Where(us => us.UserId == userId)
                .Select(us => us.SalonId)
                .ToListAsync();
        }
        
        return null;
    }

    public async Task<Domain.Entities.Salon> GetAuthenticatedSalon(Guid salonId)
    {
        var userId = _authenticationData.GetUserId();

        if (userId != null)
        {
            var isAuthenticated = await _dbContext.UserSalons
                .AnyAsync(us => us.UserId == userId && us.SalonId == salonId);

            if (!isAuthenticated)
            {
                throw new UnauthorizedException("NoAccessToThisSalon");
            }
            
            var salon = await _dbContext.Salons.FirstOrDefaultAsync(s => s.Id == salonId);
            
            if (salon == null)
            {
                throw new ErrorException("SalonDoesNotExist");
            }
            
            return salon;
        }
        
        throw new UnauthorizedException();
    }
}