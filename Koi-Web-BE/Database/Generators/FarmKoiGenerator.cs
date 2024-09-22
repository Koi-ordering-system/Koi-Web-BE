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
            .Generate(50)
            .DistinctBy(fk=>new{fk.FarmId, fk.KoiId})];
}