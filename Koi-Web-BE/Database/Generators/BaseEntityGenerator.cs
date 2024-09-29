using Bogus;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Database.Generators;

public static class BaseEntityGenerator
{
    public static Faker<TEntity> ApplyEntitesRules<TEntity>(this Faker<TEntity> faker) where TEntity : BaseEntity
        => faker
            .UseDateTimeReference(DateTime.UtcNow)
            .RuleFor(e => e.Id, f => f.Random.Guid());
}