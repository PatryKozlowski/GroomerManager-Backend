namespace GroomerManager.Application.Common.Interfaces;

public interface ICurrentSalonProvider
{
    Task<List<Guid>?> GetSalonId();
    Task<Domain.Entities.Salon> GetAuthenticatedSalon(Guid salonId);
}