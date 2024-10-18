using Bogus;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database.Generators;

public class ReviewGenerator
{
    public static Review[] Generate(User[] users, Order[] orders)
        => [.. new Faker<Review>()
            .ApplyEntitesRules()
            .RuleFor(e=>e.UserId,f=>f.PickRandom(users).Id)
            .RuleFor(e => e.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(e => e.Rating, f => f.Random.Number(1, 5))
            .RuleFor(e => e.Content, f => f.Lorem.Paragraph())
            .Generate(200).DistinctBy(r => new { r.UserId, r.OrderId })];
}