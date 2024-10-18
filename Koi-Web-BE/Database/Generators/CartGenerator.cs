// using Bogus;
// using Koi_Web_BE.Models.Entities;

// namespace Koi_Web_BE.Database.Generators;

// public class CartGenerator
// {
//     public static Cart[] Generate(User[] users)
//         => [.. new Faker<Cart>()
//             .ApplyEntitesRules()
//             .RuleFor(e => e.UserId, f => f.PickRandom(users).Id)
//             .Generate(50).DistinctBy(c=>c.UserId)];
// }