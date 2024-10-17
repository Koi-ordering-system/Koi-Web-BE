using Bogus;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;

namespace Koi_Web_BE.Database.Generators;

public class OrderGenerator
{
    public static Order[] Generate(User[] users, Farm[] farms)
        => [.. new Faker<Order>()
            .ApplyEntitesRules()
            .RuleFor(e => e.UserId, f => f.PickRandom(users).Id)
            .RuleFor(e => e.FarmId, f => f.PickRandom(farms).Id)
            .RuleFor(e=>e.PayOSOrderCode,f=>f.Random.Long(0,100))
            .RuleFor(e => e.Price, f => f.Random.Decimal(0,1000))
            .RuleFor(e=>e.IsPaid,f=>f.Random.Bool())
            .RuleFor(e=>e.Status,f=>f.Random.Bool() ? f.PickRandom<OrderStatusEnum>() : null!)
            .Generate(50).DistinctBy(o=>new {o.UserId,o.FarmId})];
}