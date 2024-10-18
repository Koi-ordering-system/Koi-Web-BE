using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class KoiGenerator
{
    public static Koi[] Generate()
    => [.. new Faker<Koi>()
        .ApplyEntitesRules()
        .RuleFor(k=>k.Name,f=>f.PickRandom(koiNames))
        .RuleFor(k=>k.Description,f=>f.Lorem.Paragraph())
        .RuleFor(k=>k.MinSize,f=>f.Random.Number(1, 10))
        .RuleFor(k=>k.MaxSize,f=>f.Random.Number(11, 100))
        .RuleFor(k=>k.Price,f=>f.PickRandom(5000,10000,20000,30000,40000,50000))
        .Generate(50)
        .DistinctBy(k=>k.Name)
    ];
    private readonly static string[] koiNames = [
    "Kohaku",
    "Tancho",
    "Sanke",
    "Showa",
    "Utsuri",
    "Bekko",
    "Goromo",
    "Yamabuki Oranda",
    "Tosai",
    "Nishikigoi",
    "Gin Matsuba",
    "Kikusui",
    "Goshiki",
    "Ajiki",
    "Matsuba",
    "Ochiba",
    "Hiutsuri",
    "Kujaku",
    "Kikokyu",
    "Chagoi",
    "Kijiki",
    "Hina Matsuba",
    "Kageyama",
    "Tancho Showa",
    "Yamabuki Oranda Showa",
    "Yamabuki Oranda Tancho",
    "Gin Matsuba Tancho",
    "Gin Matsuba Showa",
    "Kikusui Showa",
    "Kikusui Tancho",
    "Goshiki Showa",
    "Goshiki Tancho",
    "Ajiki Showa",
    "Ajiki Tancho",
    "Matsuba Showa",
    "Matsuba Tancho",
    "Ochiba Showa",
    "Ochiba Tancho",
    "Hiutsuri Showa",
    "Hiutsuri Tancho",
    "Kujaku Showa",
    "Kujaku Tancho",
    "Kikokyu Showa",
    "Kikokyu Tancho",
    "Chagoi Showa",
    "Chagoi Tancho",
    "Kijiki Showa",
    "Kijiki Tancho",
    "Hina Matsuba Showa",
    "Hina Matsuba Tancho",
    "Kageyama Showa",
    "Kageyama Tancho"
    ];
}