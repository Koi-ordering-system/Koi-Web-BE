using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Queries;

public class GetUnapprovedTrips
{
    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("api/orders/unapproved", async (ISender sender, string keyword = "", int pageIndex = 1, int pageSize = 10) =>
            {
                Result<PaginatedList<Response>> response = await sender.Send(new Query(pageIndex, pageSize), default);
                if (!response.Succeeded) return Results.Forbid();
                return Results.Ok(response);
            }).WithTags("Orders")
            .WithMetadata(new SwaggerOperationAttribute("Get Unapproved Trips"));
        }
    }

    public record Query(int PageIndex, int PageSize) : IRequest<Result<PaginatedList<Response>>>;

    public record Response(
        Guid Id,
        Guid FarmId,
        string FarmName,
        int Days,
        decimal Price
    )
    {
        public static Response FromEntity(Trip trip) => new(
            Id: trip.Id,
            FarmId: trip.FarmId,
            FarmName: trip.Farm?.Name ?? string.Empty,
            Days: trip.Days,
            Price: trip.Price
        );
    };

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Query, Result<PaginatedList<Response>>>
    {
        public async Task<Result<PaginatedList<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!(currentUser.User!.IsManager() || currentUser.User!.IsAdmin()))
                return Result<PaginatedList<Response>>.Fail(new ForbiddenException("Unauthorized."));

            IQueryable<Trip> query = context.Trips
                .AsNoTracking()
                .Include(o => o.Farm)
                .Where(o => o.IsApproved == null);
            int total = await query.CountAsync(cancellationToken);
            IEnumerable<Response> gettingTrips = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => Response.FromEntity(o))
                .ToListAsync(cancellationToken);
            return Result<PaginatedList<Response>>.Succeed(new(
                items: gettingTrips.ToList(),
                count: total,
                pageNumber: request.PageIndex,
                pageSize: request.PageSize
            ));
        }
    }
}