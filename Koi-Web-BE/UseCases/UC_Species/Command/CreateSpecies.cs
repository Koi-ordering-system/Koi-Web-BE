using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Species.Command;

public class CreateSpecies
{
    public record Command(
        string Name,
        string Description,
        int YearOfDescovery,
        string DiscoveredBy
        ) : IRequest<Result<Response>>;

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

    public class Handler(IApplicationDbContext context, IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // initialize
            Species addingSpecies = new()
            {
                Name = request.Name,
                Description = request.Description,
                YearOfDiscovery = request.YearOfDescovery,
                DiscoveredBy = request.DiscoveredBy
            };
            // add to db
            context.Species.Add(addingSpecies);
            await context.SaveChangesAsync(cancellationToken);
            // clear cache
            await store.EvictByTagAsync("Species", cancellationToken);
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
        public static async Task<IResult> Handle(ISender sender
            , CreateSpeciesRequest request
        , CancellationToken cancellationToken = default)
        {
            Result<Response> result = await sender.Send(new Command(
                Name: request.name,
                Description: request.description,
                YearOfDescovery: request.yearOfDescovery,
                DiscoveredBy: request.discoveredBy), cancellationToken);
            if (!result.Succeeded)
                return Results.BadRequest(result);
            return Results.Created("", result);
        }
    }

    public record CreateSpeciesRequest(
        string name,
        string description,
        int yearOfDescovery,
        string discoveredBy
    );
}