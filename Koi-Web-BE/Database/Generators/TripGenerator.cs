using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class TripGenerator
{
    public static Trip[] Generate(Farm[] farms)
        => [.. new Faker<Trip>()
            .ApplyEntitesRules()
            .RuleFor(e => e.FarmId, f => f.PickRandom(farms).Id)
            .RuleFor(e=>e.IsApproved,f=>f.Random.Bool()?f.Random.Bool():null!)
            .RuleFor(e=>e.Days,f=>f.Random.Number(1,7))
            .RuleFor(e=>e.Price,f=>f.PickRandom(1000,2000,5000,10000,20000,50000))
            .Generate(50)
            .DistinctBy(t => new { t.FarmId, t.Days,t.Price })];
}