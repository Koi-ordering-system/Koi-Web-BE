using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class FarmImageGenerator
{
    public static FarmImage[] Generate(Farm[] farms)
    => [.. new Faker<FarmImage>()
        .ApplyEntitesRules()
        .RuleFor(e => e.FarmId, f => f.PickRandom(farms).Id)
        .RuleFor(e => e.Url, f => f.Image.PicsumUrl())
        .Generate(1000)
        .DistinctBy(fi => new { fi.FarmId,fi.Url })];
}