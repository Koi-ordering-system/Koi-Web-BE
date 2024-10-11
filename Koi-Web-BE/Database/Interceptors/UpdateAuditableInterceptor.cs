using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Koi_Web_BE.Database.Interceptors;

public class UpdateAuditableInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditableEntities(DbContext context)
    {
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        List<EntityEntry<BaseAuditableEntity>> entities = context
            .ChangeTracker.Entries<BaseAuditableEntity>()
            .ToList();

        foreach (EntityEntry<BaseAuditableEntity> entry in entities)
        {
            if (entry.State == EntityState.Added)
            {
                SetCurrentPropertyValue(entry, nameof(BaseAuditableEntity.CreatedAt), utcNow);
            }

            if (entry.State == EntityState.Modified)
            {
                SetCurrentPropertyValue(entry, nameof(BaseAuditableEntity.UpdatedAt), utcNow);
            }
        }

        static void SetCurrentPropertyValue<T>(EntityEntry entry, string propertyName, T value) =>
            entry.Property(propertyName).CurrentValue = value;
    }
}
