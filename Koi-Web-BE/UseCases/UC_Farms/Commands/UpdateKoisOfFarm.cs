using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Farms.Commands;

public class UpdateKoisOfFarm
{

    public record Command(
        Guid Id,
        UpdateKoisOfFarmRequest[] Kois
    ) : IRequest<Result<Response>>;

    public record Response();

    public record UpdateKoisOfFarmRequest(
        Guid Id,
        int Quantity
    );

    public class Handler(IApplicationDbContext context, IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // check farm exists
            Farm? checkingFarm = await context.Farms.AsNoTracking().SingleOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (checkingFarm is null) return Result<Response>.Fail(new NotFoundException("Farm not found."));

            // check kois exists
            bool isKoisExist = await context.Kois
                .AsNoTracking()
                .AllAsync(k => request.Kois.Select(r => r.Id).Contains(k.Id), cancellationToken);
            if (!isKoisExist) return Result<Response>.Fail(new NotFoundException("Kois not found."));
            // delete all farm kois that are exists
            await context.FarmKois.Where(fk => fk.FarmId == request.Id)
                .ExecuteDeleteAsync(cancellationToken);
            // add new farm kois
            await context.FarmKois.AddRangeAsync(request.Kois.Select(r => new FarmKoi
            {
                FarmId = request.Id,
                KoiId = r.Id,
                Quantity = r.Quantity
            }), cancellationToken);
            // save
            await context.SaveChangesAsync(cancellationToken);
            await store.EvictByTagAsync("Farms", cancellationToken);
            await store.EvictByTagAsync("Kois", cancellationToken);
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/farms/{id}/kois", async (ISender sender, Guid id, UpdateKoisOfFarmRequest[] request) =>
            {
                Result<Response> result = await sender.Send(new Command(id, request), default);
                if (!result.Succeeded) return Results.BadRequest(result);
                return Results.NoContent();
            }).WithTags("Farms")
            .WithMetadata(new SwaggerOperationAttribute("Update Kois of Farm"))
            .RequireAuthorization();
        }
    }
}