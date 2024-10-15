using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class FarmGenerator
{
    public static Farm[] Generate()
        => [.. new Faker<Farm>()
            .ApplyEntitesRules()
            .RuleFor(f=>f.Name, f=>f.PickRandom(koiFarms))
            .RuleFor(f=>f.Owner,f=>f.Name.FullName())
            .RuleFor(f=>f.Address,f=>f.Address.FullAddress())
            .RuleFor(f=>f.Description,f=>f.Lorem.Paragraph())
            .RuleFor(f=>f.Rating,f=>f.Random.Decimal(0,5))
            .Generate(50)
            .DistinctBy(f=>f.Name)];

    private readonly static string[] koiFarms = [
            "Yamamatsu Koi Farm",
            "Omosako Koi Farm",
            "Dainichi Koi Farm",
            "Marusei Koi Farm",
            "Momotaro Koi Farm",
            "Izumiya Koi Farm",
            "Sakai Koi Farm",
            "Matsue Koi Farm",
            "Shintaro Koi Farm",
            "Maruyama Koi Farm",
            "Hoshikin Koi Farm",
            "Tanaka Koi Farm",
            "Kobayashi Koi Farm",
            "Nakamura Koi Farm",
            "Suzuki Koi Farm",
            "Yamada Koi Farm",
            "Fujimoto Koi Farm",
            "Kawasaki Koi Farm",
            "Sato Koi Farm",
            "Honda Koi Farm",
            "Mori Koi Farm",
            "Inoue Koi Farm",
            "Hashimoto Koi Farm",
            "Yoshida Koi Farm",
            "Nakajima Koi Farm",
            "Ogawa Koi Farm",
            "Kato Koi Farm",
            "Watanabe Koi Farm",
            "Ueda Koi Farm",
            "Takeuchi Koi Farm",
            "Endo Koi Farm",
            "Kaneko Koi Farm",
            "Sakaiya Koi Farm",
            "Nagoya Koi Farm",
            "Kyoto Koi Farm",
            "Osaka Koi Farm",
            "Hiroshima Koi Farm",
            "Fukuoka Koi Farm",
            "Sapporo Koi Farm",
            "Sendai Koi Farm",
            "Chiba Koi Farm",
            "Shizuoka Koi Farm",
            "Gifu Koi Farm",
            "Okayama Koi Farm",
            "Kanazawa Koi Farm",
            "Nagasaki Koi Farm",
            "Kumamoto Koi Farm",
            "Kagoshima Koi Farm"
    ];
}