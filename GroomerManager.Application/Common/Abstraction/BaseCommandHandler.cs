using GroomerManager.Application.Common.Interfaces;

namespace GroomerManager.Application.Common.Abstraction;

public abstract class BaseCommandHandler
{
    private readonly IGroomerManagerDbContext _dbContext;
    private readonly ICurrentSalonProvider _currentSalon;
    public BaseCommandHandler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon)
    {
        _dbContext = dbContext;
        _currentSalon = currentSalon;
    }
}