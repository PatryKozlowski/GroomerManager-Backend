using GroomerManager.Application.Common.Interfaces;

namespace GroomerManager.Infrastructure.Services;

public class DataTimeService : IDateTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}