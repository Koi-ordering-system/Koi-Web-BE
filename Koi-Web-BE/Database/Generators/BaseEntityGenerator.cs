using Bogus;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Database.Generators;

public static class BaseEntityGenerator
{
    public static Faker<TEntity> ApplyEntitesRules<TEntity>(this Faker<TEntity> faker) where TEntity : BaseEntity
            => faker
            .UseDateTimeReference(DateTime.UtcNow)
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.CreatedAt, f => f.Date.Past())
            .RuleFor(e => e.UpdatedAt, f => f.Date.Past())
            .RuleFor(e => e.DeletedAt, f => f.Random.Bool() ? f.Date.Past() : null)
            .RuleFor(e => e.IsDeleted, (f, u) => u.DeletedAt != null);
}