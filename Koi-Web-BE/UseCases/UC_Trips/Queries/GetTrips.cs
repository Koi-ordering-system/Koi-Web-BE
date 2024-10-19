using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Trips.Queries;

public class GetTrips
{
    public record Query(
        Guid? farmId,
        Guid? koiId,
        decimal? minPrice,
        decimal? maxPrice,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        int pageIndex = 1,
        int pageSize = 10,
        string keyword = ""
    ) : IRequest<Result<PaginatedList<Response>>>;

    public record Response(
        Guid Id,
        Guid FarmId,
        string FarmName,
        string[] FarmImages,
        KoiDetails[] KoiDetails,
        int Days,
        decimal Price
    )
    {

        public static Response FromEntity(Trip trip) => new(
            Id: trip.Id,
            FarmId: trip.FarmId,
            FarmName: trip.Farm.Name,
            FarmImages: trip.Farm.FarmImages.Select(i => i.Url).ToArray(),
            KoiDetails: trip.Farm.FarmKois.Select(fk => new KoiDetails(fk.KoiId,
             fk.Koi.Name)).ToArray(),
            Days: trip.Days,
            Price: trip.Price
        );
    };

    public record KoiDetails(
        Guid Id,
        string Name
    );


    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<PaginatedList<Response>>>
    {
        public async Task<Result<PaginatedList<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Trip> query = context.Trips
                .AsNoTracking()
                .Include(t => t.Farm).ThenInclude(f => f.FarmImages)
                .Include(t => t.Farm).ThenInclude(f => f.FarmKois).ThenInclude(fk => fk.Koi)
                .Where(t => t.IsApproved == true);

            if (request.farmId is not null)
                query = query.Where(t => t.FarmId == request.farmId);

            if (request.koiId is not null)
                query = query.Where(t => t.Farm.FarmKois.Any(fk => fk.KoiId == request.koiId));

            if (request.minPrice is not null)
                query = query.Where(t => t.Price >= request.minPrice);

            if (request.maxPrice is not null)
                query = query.Where(t => t.Price <= request.maxPrice);

            if (request.startDate is not null && request.endDate is not null)
                query = query.Where(t => t.Days == (request.endDate - request.startDate)!.Value.Days);

            if (!string.IsNullOrEmpty(request.keyword))
                query = query.Where(t => EF.Functions.ILike(t.Farm.Name, $"%{request.keyword}%"));

            int total = await query.CountAsync(cancellationToken);
            IEnumerable<Response> gettingTrips = await query
                .Skip((request.pageIndex - 1) * request.pageSize)
                .Take(request.pageSize)
                .Select(t => Response.FromEntity(t))
                .ToListAsync(cancellationToken);
            return Result<PaginatedList<Response>>.Succeed(new(
                gettingTrips.ToList(),
                 total,
                 request.pageIndex,
                 request.pageSize));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/trips", Handle)
            .WithTags("Trips")
            .WithMetadata(new SwaggerOperationAttribute("Get all Trips"))
            .CacheOutput(b => b.Tag("Trips"));
        }
        public static async Task<IResult> Handle(ISender sender, [AsParameters] Query request, CancellationToken cancellationToken = default)
        {
            Result<PaginatedList<Response>> response = await sender.Send(request, cancellationToken);
            return Results.Ok(response);
        }

    }
}