using GroomerManager.Domain.Common;

namespace GroomerManager.Domain.ValueObjects;

public class UserName : ValueObject
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}