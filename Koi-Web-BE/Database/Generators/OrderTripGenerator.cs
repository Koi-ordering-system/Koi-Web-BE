using Bogus;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;

namespace Koi_Web_BE.Database.Generators;

public class OrderTripGenerator
{
    public static OrderTrip[] Generate(Order[] orders, Trip[] trips)
        => [.. new Faker<OrderTrip>()
            .ApplyEntitesRules()
            .RuleFor(e => e.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(e => e.TripId, f => f.PickRandom(trips).Id)
            .RuleFor(e=>e.StartDate,f=>f.Date.Past())
            .RuleFor(e=>e.EndDate,(f,u)=>u.StartDate + TimeSpan.FromDays(trips.First(t=>t.Id==u.TripId).Days))
            .RuleFor(e => e.Status, f => f.PickRandom<TripStatusEnum>())
            .Generate(100)
            .DistinctBy(ot => new { ot.OrderId, ot.TripId })];
}