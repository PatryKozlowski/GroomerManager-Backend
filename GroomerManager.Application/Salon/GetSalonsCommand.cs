using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Salon;

public abstract class GetSalonsCommand
{
    public class GetSalonsRequest : IRequest<List<GetSalonsResponse>>
    {
        
    }
    
    public class GetSalonsResponse
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string? LogoPath { get; set; }
        public required bool IsDefault { get; set; }
    }

    public class Handler : BaseCommandHandler, IRequestHandler<GetSalonsRequest, List<GetSalonsResponse>>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly IAuthenticationDataProvider _authenticationData;
        
        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IAuthenticationDataProvider authenticationData) : base(dbContext, currentSalon)
        {
            _dbContext = dbContext;
            _authenticationData = authenticationData;
        }

        public async Task<List<GetSalonsResponse>> Handle(GetSalonsRequest request, CancellationToken cancellationToken)
        {
            var userId = _authenticationData.GetUserId();
            
            if (userId == null)
            {
                throw new UnauthorizedException();
            }
            
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.UserSalons)
                .ThenInclude(us => us.Salon)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new ErrorException("UserNotFound");
            }
            
            List<GetSalonsResponse> salonDtos;

            if (user.Role.Name == "Owner")
            {
                var ownerSalons = await _dbContext.Salons
                    .Where(s => s.OwnerId == userId)
                    .ToListAsync(cancellationToken);

                salonDtos = ownerSalons.Select(salon => new GetSalonsResponse
                {
                    Id = salon.Id,
                    Name = salon.Name,
                    LogoPath = salon.LogoPath,
                    IsDefault = salon.IsDefault
                }).ToList();
            }
            else
            {
                var employeeSalons = user.UserSalons.Select(us => us.Salon);
                
                salonDtos = employeeSalons.Select(salon => new GetSalonsResponse
                {
                    Id = salon.Id,
                    Name = salon.Name,
                    LogoPath = salon.LogoPath,
                    IsDefault = salon.IsDefault
                }).ToList();
            }
            
            return salonDtos;
        }
    }
}