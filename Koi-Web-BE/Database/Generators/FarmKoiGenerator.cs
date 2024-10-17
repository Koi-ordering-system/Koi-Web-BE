using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class FarmKoiGenerator
{
    public static FarmKoi[] Generate(Farm[] farms, Koi[] kois)
        => [.. new Faker<FarmKoi>()
            .ApplyEntitesRules()
            .RuleFor(e => e.FarmId, f => f.PickRandom(farms).Id)
            .RuleFor(e => e.KoiId, f => f.PickRandom(kois).Id)
            .RuleFor(e => e.Quantity, f => f.Random.Number(1, 1000))
            .Generate(1000)
            .DistinctBy(fk=>new{fk.FarmId, fk.KoiId})];
}