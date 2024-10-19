// using Bogus;
// using Koi_Web_BE.Models.Entities;

// namespace Koi_Web_BE.Database.Generators;

// public class CartItemGenerator
// {
//     public static CartItem[] Generate(Cart[] carts, FarmKoi[] farmKois, Koi[] kois, Color[] colors)
//         => [.. new Faker<CartItem>()
//             .ApplyEntitesRules()
//             .RuleFor(e => e.CartId, f => f.PickRandom(carts).Id)
//             .RuleFor(e => e.FarmKoiId, f => f.PickRandom(farmKois).Id)
//             .RuleFor(e => e.Quantity, (f,u) =>
//                 f.Random.Number(1, farmKois.First(k=>k.Id==u.FarmKoiId).Quantity))
//             .RuleFor(e => e.Color, (f, u) =>
//                 f.PickRandom(colors.Where(c => c.KoiId == farmKois.First(fk => fk.Id == u.FarmKoiId).KoiId)).Name)
//             .RuleFor(e => e.Size, (f,u) =>
//                     f.Random.Decimal(kois.First(k=>
//                         farmKois.First(fk=>fk.Id==u.FarmKoiId).KoiId.Equals(k.Id)).MinSize,
//                         kois.First(k=>farmKois.First(fk=>fk.Id==u.FarmKoiId).KoiId.Equals(k.Id)).MaxSize
//                     )
//                 )
//             .Generate(50)
//             .DistinctBy(ci=>new {ci.CartId, ci.FarmKoiId})];
// }