using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Species.Command;

public class CreateSpecies
{
    public record Command(string Name) : IRequest<Result<Response>>;

    public record Response(
        Guid Id,
        string Name
    )
    {
        public static Response FromEntity(Species species)
            => new(
                Id: species.Id,
                Name: species.Name
            );
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // initialize
            Species addingSpecies = new()
            {
                Name = request.Name,
            };
            // add to db
            context.Species.Add(addingSpecies);
            await context.SaveChangesAsync(cancellationToken);
            // return result
            return Result<Response>.Succeed(Response.FromEntity(addingSpecies));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("api/species", Handle)
            .WithTags("Species")
            .WithMetadata(new SwaggerOperationAttribute("Create a Species"))
            .RequireAuthorization();
        }
        public static async Task<IResult> Handle(ISender sender, string name, CancellationToken cancellationToken = default)
        {
            Result<Response> result = await sender.Send(new Command(Name: name), cancellationToken);
            if (!result.Succeeded)
                return Results.BadRequest(result);
            return Results.Created("", result);
        }
    }
}