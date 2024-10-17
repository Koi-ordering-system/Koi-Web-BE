using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class KoiImageGenerator
{
    public static KoiImage[] Generate(Koi[] kois)
        => [.. new Faker<KoiImage>()
            .ApplyEntitesRules()
            .RuleFor(e => e.KoiId, f => f.PickRandom(kois).Id)
            .RuleFor(e => e.Url, f => f.Image.PicsumUrl())
            .Generate(1000)
            .DistinctBy(fi => new { fi.KoiId,fi.Url })];
}