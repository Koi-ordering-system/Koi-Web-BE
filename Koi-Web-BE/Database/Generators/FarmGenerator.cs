using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class FarmGenerator
{
    public static Farm[] Generate()
        => [.. new Faker<Farm>()
            .ApplyEntitesRules()
            .RuleFor(f=>f.Name, f=>f.Commerce.ProductName())
            .RuleFor(f=>f.Owner,f=>f.Name.FullName())
            .RuleFor(f=>f.Address,f=>f.Address.FullAddress())
            .RuleFor(f=>f.Description,f=>f.Lorem.Paragraph())
            .RuleFor(f=>f.Rating,f=>f.Random.Decimal(0,5))
            .Generate(50)];
}