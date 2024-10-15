using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Species.Command;

public class UpdateSpecies
{
    public record Command(Guid Id,
        string Name,
        string Description,
        int YearOfDescovery,
        string DiscoveredBy) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context, IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context.Species
                .Where(s => s.Id.Equals(request.Id))
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(e => e.Name, request.Name)
                        .SetProperty(e => e.Description, request.Description)
                        .SetProperty(e => e.YearOfDiscovery, request.YearOfDescovery)
                        .SetProperty(e => e.DiscoveredBy, request.DiscoveredBy)
                        .SetProperty(e => e.UpdatedAt, DateTime.UtcNow)
                    , cancellationToken);
            if (result == 0) return Result<Response>.Fail(new NotFoundException("Species not found."));
            // clear cache
            await store.EvictByTagAsync("Species", cancellationToken);
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPut("api/species/{id}", Handle)
            .WithTags("Species")
            .WithMetadata(new SwaggerOperationAttribute("Update a Species"))
            .RequireAuthorization();
        }
    }
    public static async Task<IResult> Handle(ISender sender, Guid id,
        UpdateSpeciesRequest request,
     CancellationToken cancellationToken = default)
    {
        Result<Response> result = await sender.Send(new Command(id, request.name,
            request.description, request.yearOfDescovery, request.discoveredBy), cancellationToken);
        if (!result.Succeeded)
            return Results.BadRequest(result);
        return Results.NoContent();
    }

    public class UpdateSpeciesRequest()
    {
        public string name { get; set; } = "";
        public string description { get; set; } = "";
        public int yearOfDescovery { get; set; } = 0;
        public string discoveredBy { get; set; } = "";
    }
}