using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Species.Queries;

public class GetSpeciesById
{
    public record Query(Guid Id) : IRequest<Result<Response>>;

    public record Response(Guid Id, string Name,
        string Description,
        int YearOfDiscovery,
        string DiscoveredBy,
        DateTimeOffset CreatedAt
    )
    {
        public static Response FromEntity(Species species)
            => new(species.Id, species.Name, species.Description, species.YearOfDiscovery, species.DiscoveredBy, species.CreatedAt);
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            Species? gettingSpecies = await context.Species
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Id.Equals(request.Id), cancellationToken);
            if (gettingSpecies == null) return Result<Response>.Fail(new NotFoundException("Species not found."));
            return Result<Response>.Succeed(Response.FromEntity(gettingSpecies));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("api/species/{id}", Handle)
            .WithTags("Species")
            .WithMetadata(new SwaggerOperationAttribute("Get a Species"))
            .CacheOutput(b => b.Tag("Species"));
        }
        public static async Task<IResult> Handle(ISender sender, Guid id, CancellationToken cancellationToken = default)
        {
            Result<Response> query = await sender.Send(new Query(id), cancellationToken);
            if (!query.Succeeded) return Results.NotFound(query);
            return Results.Ok(query);
        }
    }
}