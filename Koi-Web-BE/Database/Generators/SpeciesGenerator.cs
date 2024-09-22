using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class SpeciesGenerator
{
    public static Species[] Generate()
        => [.. new Faker<Species>()
            .ApplyEntitesRules()
            .RuleFor(e=>e.Name,f=>f.Commerce.ProductName())
            .Generate(50)];
}