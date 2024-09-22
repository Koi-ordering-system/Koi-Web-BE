using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class OrderKoiGenerator
{
    public static OrderKoi[] Generate(Order[] orders, Koi[] kois)
        => [.. new Faker<OrderKoi>()
            .ApplyEntitesRules()
            .RuleFor(k => k.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(k => k.KoiId, f => f.PickRandom(kois).Id)
            .RuleFor(k=>k.Quantity,f=>f.Random.Number(1,100))
            .Generate(50)
            .DistinctBy(ok=>new {ok.OrderId, ok.KoiId})];
}