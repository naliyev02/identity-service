namespace Identity.Domain.Common;
public abstract class AuditableEntity : BaseEntity
{
    public Guid CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public void SetCreatedBy(Guid userId)
    {
        CreatedBy = userId;
    }
    public void SetUpdatedBy(Guid userId)
    {
        UpdatedBy = userId;
        SetUpdatedAt(); 
    }
}
