using Koi_Web_BE.Database;
using Koi_Web_BE.Models;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.UseCases.UC_Trips.Queries;

public class GetUnapprovedTrips
{
    public record Query(
        int PageIndex = 1,
        int PageSize = 10,
        string Keyword = ""
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
                .Where(t => t.IsApproved == false);

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

}