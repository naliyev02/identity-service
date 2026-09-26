namespace Identity.Domain.Common;
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
}
