using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class CartItemGenerator
{
    public static CartItem[] Generate(Cart[] carts, FarmKoi[] farmKois)
        => [.. new Faker<CartItem>()
            .ApplyEntitesRules()
            .RuleFor(e => e.CartId, f => f.PickRandom(carts).Id)
            .RuleFor(e => e.FarmKoiId, f => f.PickRandom(farmKois).Id)
            .RuleFor(e => e.Quantity, f => f.Random.Number(1, 100))
            .Generate(50)
            .DistinctBy(ci=>new {ci.CartId, ci.FarmKoiId})];
}