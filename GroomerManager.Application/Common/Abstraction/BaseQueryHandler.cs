using GroomerManager.Application.Common.Interfaces;

namespace GroomerManager.Application.Common.Abstraction;

public abstract class BaseQueryHandler
{
    private readonly IGroomerManagerDbContext _dbContext;
    private readonly ICurrentSalonProvider _currentSalon;

    public BaseQueryHandler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon)
    {
        _dbContext = dbContext;
        _currentSalon = currentSalon;
    }
}