using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace Koi_Web_BE.UseCases.UC_Trips.Queries;

public class GetTrips
{
    public record Query(
        int PageIndex = 1,
        int PageSize = 10,
        string Keyword = "",
        Guid? FarmId = null,
        Guid? KoiId = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        DateTimeOffset? StartDate = null,
        DateTimeOffset? EndDate = null
    ) : IRequest<Result<PaginatedList<Response>>>;

    public record GetTripsRequest(
        int pageIndex = 1,
        int pageSize = 10,
        string keyword = "",
        Guid? farmId = null,
        Guid? koiId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null
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

            if (request.FarmId is not null)
                query = query.Where(t => t.FarmId == request.FarmId);

            if (request.KoiId is not null)
                query = query.Where(t => t.Farm.FarmKois.Any(fk => fk.KoiId == request.KoiId));

            if (request.MinPrice is not null)
                query = query.Where(t => t.Price >= request.MinPrice);

            if (request.MaxPrice is not null)
                query = query.Where(t => t.Price <= request.MaxPrice);

            if (request.StartDate is not null && request.EndDate is not null)
                query = query.Where(t => t.Days == (request.EndDate - request.StartDate)!.Value.Days);

            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(t => EF.Functions.ILike(t.Farm.Name, $"%{request.Keyword}%"));

            int total = await query.CountAsync(cancellationToken);
            IEnumerable<Response> gettingTrips = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => Response.FromEntity(t))
                .ToListAsync(cancellationToken);
            return Result<PaginatedList<Response>>.Succeed(new(
                gettingTrips.ToList(),
                 total,
                 request.PageIndex,
                 request.PageSize));
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
        public static async Task<IResult> Handle(ISender sender, [AsParameters] GetTripsRequest request, CancellationToken cancellationToken = default)
        {
            Result<PaginatedList<Response>> response = await sender.Send(new Query(
                request.pageIndex,
                request.pageSize,
                request.keyword,
                request.farmId,
                request.koiId,
                request.minPrice,
                request.maxPrice,
                request.startDate,
                request.endDate
            ), cancellationToken);
            return Results.Ok(response);
        }

    }
}