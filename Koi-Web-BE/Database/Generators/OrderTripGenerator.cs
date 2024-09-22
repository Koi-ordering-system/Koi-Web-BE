using Bogus;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;

namespace Koi_Web_BE.Database.Generators;

public class OrderTripGenerator
{
    public static OrderTrip[] Generate(Order[] orders)
        => [.. new Faker<OrderTrip>()
            .ApplyEntitesRules()
            .RuleFor(e => e.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(e=>e.StartDate,f=>f.Date.Past())
            .RuleFor(e=>e.EndDate,f=>f.Date.Future())
            .RuleFor(e => e.Status, f => f.PickRandom<TripStatusEnum>())
            .Generate(50)
            .DistinctBy(ot => new { ot.OrderId })];
}