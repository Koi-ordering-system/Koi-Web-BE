using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Species.Command;

public class UpdateSpecies
{
    public record Command(Guid Id, string Name) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context,IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context.Species
                .Where(s => s.Id.Equals(request.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Name, request.Name), cancellationToken);
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
    public static async Task<IResult> Handle(ISender sender, Guid id, string name, CancellationToken cancellationToken = default)
    {
        Result<Response> result = await sender.Send(new Command(id, name), cancellationToken);
        if (!result.Succeeded)
            return Results.BadRequest(result);
        return Results.NoContent();
    }
}