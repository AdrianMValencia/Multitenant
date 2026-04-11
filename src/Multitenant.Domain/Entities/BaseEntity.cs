namespace Multitenant.Domain.Entities;

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
