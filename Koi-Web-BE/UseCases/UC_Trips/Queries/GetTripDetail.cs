using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace Koi_Web_BE.UseCases.UC_Trips.Queries;

public class GetTripDetail
{
    public record Query(
        Guid Id
    ) : IRequest<Result<Response>>;

    public record GetTripsDetailRequest(
        Guid id
    );

    public record Response(
        Guid Id,
        Guid FarmId,
        string FarmName,
        string[] FarmImages,
        string FarmOwner,
        string FarmAddress,
        string FarmDescription,
        decimal FarmRating,
        int Days,
        decimal Price,
        KoiDetails[] KoiDetails
    )
    {
        public static Response FromEntity(Trip trip) => new(
            Id: trip.Id,
            FarmId: trip.FarmId,
            FarmName: trip.Farm.Name,
            FarmImages: trip.Farm.FarmImages.Select(i => i.Url).ToArray(),
            FarmOwner: trip.Farm.Owner,
            FarmAddress: trip.Farm.Address,
            FarmRating: trip.Farm.Rating,
            Days: trip.Days,
            Price: trip.Price,
            KoiDetails: trip.Farm.FarmKois.Select(static fk => new KoiDetails(
                Id: fk.KoiId,
                Name: fk.Koi.Name,
                Images: fk.Koi.Images.Select(i => i.Url).ToArray(),
                Description: fk.Koi.Description,
                MinSize: fk.Koi.MinSize,
                MaxSize: fk.Koi.MaxSize,
                Price: fk.Koi.Price,
                Quantity: fk.Quantity
            )).ToArray(),
            FarmDescription: trip.Farm.Description);
    };

    public record KoiDetails(
        Guid Id,
        string Name,
        string[] Images,
        string Description,
        decimal MinSize,
        decimal MaxSize,
        decimal Price,
        int Quantity
    );

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            Trip? trip = await context.Trips
                .AsNoTracking()
                .Include(t => t.Farm).ThenInclude(f => f.FarmImages)
                .Include(t => t.Farm).ThenInclude(f => f.FarmKois).ThenInclude(fk => fk.Koi).ThenInclude(f => f.Images)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            return trip is null ? Result<Response>.Fail(new NotFoundException("Trip not found.")) : Result<Response>.Succeed(Response.FromEntity(trip));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/trips/{id:guid}", async (ISender sender, [AsParameters] GetTripsDetailRequest request) =>
                {
                    Result<Response> response = await sender.Send(new Query(request.id), default);
                    if (!response.Succeeded) return Results.NotFound(response);
                    return Results.Ok(response);
                })
                .WithTags("Trips")
                .WithMetadata(new SwaggerOperationAttribute("Get a Trip"))
                .CacheOutput(b => b.Tag("Trips"));
        }

    }
}
