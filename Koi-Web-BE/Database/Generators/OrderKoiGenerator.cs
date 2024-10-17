using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class OrderKoiGenerator
{
    public static OrderKoi[] Generate(Order[] orders, Koi[] kois, Color[] colors)
        => [.. new Faker<OrderKoi>()
            .ApplyEntitesRules()
            .RuleFor(k => k.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(k => k.KoiId, f => f.PickRandom(kois).Id)
            .RuleFor(k=>k.Quantity,f=>f.Random.Number(1,100))
            .RuleFor(e => e.Size, (f,u) =>
                    f.Random.Decimal(kois.First(k=>
                        k.Id==u.KoiId).MinSize,
                        kois.First(k=>k.Id==u.KoiId).MaxSize
                    )
                )
            .RuleFor(e => e.Color, (f, u) =>
                f.PickRandom(colors.Where(c => c.KoiId == u.KoiId)).Name)
            .Generate(1000)
            .DistinctBy(ok=>new {ok.OrderId, ok.KoiId})];
}