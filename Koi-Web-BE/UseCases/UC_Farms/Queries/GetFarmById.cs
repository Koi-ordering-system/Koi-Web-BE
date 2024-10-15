using Bogus.DataSets;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sprache;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Farms.Queries;

public class GetFarmById
{
    public record Query(
        Guid Id
    ) : IRequest<Result<Response>>;

    public record Response(
        string Name,
        string Owner,
        string Address,
        string Description,
        decimal Rating,
        IEnumerable<string> FarmImages
    )
    {
        public static Response FromEntity(Farm farm)
            => new(
                Name: farm.Name,
                Owner: farm.Owner,
                Address: farm.Address,
                Description: farm.Description,
                Rating: farm.Rating,
                FarmImages: farm.FarmImages.Select(farmImage => farmImage.Url)
            );
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            Farm? farm = await context.Farms
                .AsNoTracking()
                .Include(f => f.FarmImages)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (farm is null)
                return Result<Response>.Fail(new NotFoundException("Farm not found."));
            return Result<Response>.Succeed(Response.FromEntity(farm));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("api/farms/{id}", Handle)
                .WithTags("Farms")
                .WithMetadata(new SwaggerOperationAttribute("Get a Farm"))
                .CacheOutput(b => b.Tag("Farms"));
        }

        public static async Task<IResult> Handle(ISender sender, Guid id, CancellationToken cancellationToken = default)
        {
            Result<Response> query = await sender.Send(new Query(id), cancellationToken);
            if (!query.Succeeded) return Results.NotFound(query);
            return Results.Ok(query);
        }
    }
}