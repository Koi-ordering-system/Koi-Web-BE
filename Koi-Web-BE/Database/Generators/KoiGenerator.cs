using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class KoiGenerator
{
    public static Koi[] Generate(Species[] species)
    => [.. new Faker<Koi>()
        .ApplyEntitesRules()
        .RuleFor(k=>k.SpeciesId,f=>f.PickRandom(species).Id)
        .RuleFor(k=>k.Name,f=>f.Commerce.ProductName())
        .RuleFor(k=>k.Description,f=>f.Lorem.Paragraph())
        .RuleFor(k=>k.MinSize,f=>f.Random.Number(1, 10))
        .RuleFor(k=>k.MaxSize,f=>f.Random.Number(11, 100))
        .RuleFor(k=>k.IsMale,f=>f.Random.Bool())
        .RuleFor(k=>k.Price,f=>f.Random.Decimal(1,100))
        .Generate(50)
    ];
}