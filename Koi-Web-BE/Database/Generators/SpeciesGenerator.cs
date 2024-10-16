using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class SpeciesGenerator
{
    public static Species[] Generate()
        => [.. new Faker<Species>()
            .ApplyEntitesRules()
            .UseDateTimeReference(DateTime.UtcNow)
            .RuleFor(e=>e.Name,f=>f.PickRandom(koiVarieties))
            .RuleFor(e=>e.Description,f=>f.Lorem.Sentence())
            .RuleFor(e=>e.YearOfDiscovery,f=>f.Random.Number(0,2024))
            .RuleFor(e=>e.DiscoveredBy,f=>f.Name.FullName())
            .RuleFor(e=>e.CreatedAt,f=>f.Date.Past())
            .RuleFor(e=>e.UpdatedAt,f=>f.Random.Bool() ? f.Date.Past() : null!)
            .Generate(50).DistinctBy(s=>s.Name)];

    private static readonly string[] koiVarieties = [
            "Kohaku",
            "Taisho Sanke",
            "Showa Sanshoku",
            "Bekko",
            "Utsurimono",
            "Asagi",
            "Shiro Utsuri",
            "Goshiki",
            "Koromo",
            "Doitsu",
            "Tancho",
            "Hikarimuji",
            "Kikokuryu",
            "Ogon",
            "Konikoi",
            "Platinum Ogon",
            "Lionhead",
            "Genho",
            "Yamabuki Ogon",
            "Seigaiha",
            "Sanke Showa",
            "Sanke Taisho",
            "Shirokoi",
            "Hi Utsuri",
            "Ki Utsuri",
            "Gin Rin",
            "Black Hinaki",
            "Red Hinaki",
            "Kikenkoi",
            "Biwako Ogon",
            "Toranomon Ogon",
            "Tatsuno Sanke",
            "Kumonryu",
            "Beni Asagi",
            "Shiro Asagi",
            "Gindara",
            "Sanke Mixed",
            "Showa Mixed",
            "Omote Taisho Sanke",
            "Omote Showa Sanshoku",
            "Hikari Showa",
            "Beni Kohaku",
            "Bi-Kohaku",
            "Shiro Showa",
            "Utsuri Sanshoku",
            "Hikari Asagi",
            "Kohaku Asagi",
            "Mizu Utsuri",
            "Ogon Mixed",
            "Tawara Sanke"
    ];
}