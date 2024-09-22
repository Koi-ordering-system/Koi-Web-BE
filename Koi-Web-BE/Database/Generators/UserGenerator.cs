using Bogus;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;

namespace Koi_Web_BE.Database.Generators;

public class UserGenerator
{
    public static User[] Generate()
        => [.. new Faker<User>()
            .ApplyEntitesRules()
            .RuleFor(e=>e.Id,f=>f.Random.Word())
            .RuleFor(e=>e.Username,f=>f.Internet.UserName())
            .RuleFor(e=>e.AvatarUrl,f=>f.Internet.Avatar())
            .RuleFor(e=>e.Email,f=>f.Internet.Email())
            .RuleFor(e=>e.PhoneNumber,f=>f.Phone.PhoneNumber())
            .RuleFor(e=>e.Role,f=>f.PickRandom<RoleEnum>())
            .Generate(50).DistinctBy(u=>u.Id)];
}