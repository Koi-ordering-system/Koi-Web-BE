using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Models.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Users.Commands;

public class CreateUser
{
    public record Command(Event AddingEvent) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            ClerkUser clerkUser = request.AddingEvent.Data!;
            User? checkingUser = await context.Users.FirstOrDefaultAsync(u => u.Id.Equals(clerkUser.Id), cancellationToken);
            if (checkingUser is not null) return Result<Response>.Succeed(null!);
            User user = new()
            {
                Id = clerkUser.Id ?? string.Empty,
                Username = clerkUser.Username ?? string.Empty,
                Email = clerkUser.EmailAddresses.FirstOrDefault()?.EmailAddress ?? string.Empty,
                PhoneNumber = clerkUser.PhoneNumbers.FirstOrDefault()?.PhoneNumber ?? string.Empty,
                AvatarUrl = clerkUser.ImageUrl ?? string.Empty,
                Role = Models.Enums.RoleEnum.Customer
            };
            if (user.Id.Equals(string.Empty))
                return Result<Response>.Fail(new Exception("User is not found !"));
            await context.Users.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Result<Response>.Succeed(null!);
            // return Result.Success();
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users", async (ISender sender, Event request, CancellationToken cancellationToken = default) =>
            {
                Result<Response> result = await sender.Send(new Command(request), cancellationToken);
                return Results.Ok(result);
            })
            .WithMetadata(new SwaggerOperationAttribute("Webhook for creating a User"))
            .WithTags("Users");
        }
    }
}