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
        IEnumerable<FarmImageResponse> FarmImages,
        IEnumerable<KoiResponse> Kois,
        IEnumerable<TripResponse> Trips
    )
    {
        public static Response FromEntity(Farm farm)
            => new(
                Name: farm.Name,
                Owner: farm.Owner,
                Address: farm.Address,
                Description: farm.Description,
                Rating: farm.Rating,
                FarmImages: farm.FarmImages.Select(farmImage => new FarmImageResponse(farmImage.Id, farmImage.Url)),
                Kois: farm.FarmKois.Select(farmKoi => new KoiResponse(farmKoi.Koi.Id, farmKoi.Koi.Name, farmKoi.Quantity, farmKoi.Koi.Images.Select(image => image.Url))),
                Trips: farm.Trips.Select(trip => new TripResponse(trip.Id, trip.Farm.Id, trip.Farm.Name, trip.Days, trip.Price))
            );
    }

    public record FarmImageResponse(Guid Id, string Url);

    public record KoiResponse(
        Guid Id,
        string Name,
        int Quantity,
        IEnumerable<string> ImageUrls
    );

    public record TripResponse(
        Guid Id,
        Guid FarmId,
        string FarmName,
        int Days,
        decimal Price
    );

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            Farm? farm = await context.Farms
                .AsNoTracking()
                .AsSplitQuery()
                .Include(f => f.FarmImages)
                .Include(f => f.Trips)
                .Include(f => f.FarmKois).ThenInclude(f => f.Koi).ThenInclude(f => f.Images)
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