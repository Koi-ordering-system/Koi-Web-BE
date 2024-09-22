using System.ComponentModel.DataAnnotations.Schema;

namespace Koi_Web_BE.Models.Primitives;

public class BaseEntity
{
    public virtual Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; } = null!;
    public DateTimeOffset? DeletedAt { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;

    public void Delete() => DeletedAt = DateTimeOffset.UtcNow;
    public void Restore() => DeletedAt = null!;

    public List<BaseEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}