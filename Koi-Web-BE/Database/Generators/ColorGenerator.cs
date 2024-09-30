using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class ColorGenerator
{
    public static Color[] Generate(Koi[] kois)
        => [.. new Faker<Color>()
            .ApplyEntitesRules()
            .RuleFor(e => e.KoiId, f => f.PickRandom(kois).Id)
            .RuleFor(e => e.Name, f => f.Commerce.Color())
            .Generate(50)];
}