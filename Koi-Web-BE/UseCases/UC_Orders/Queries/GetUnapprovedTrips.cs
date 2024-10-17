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
        string UserId,
        string UserName,
        Guid FarmId,
        string FarmName,
        decimal Price,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate
    )
    {
        public static Response FromEntity(Order order) => new(
            Id: order.Id,
            UserId: order.UserId,
            UserName: order.User?.Username ?? string.Empty,
            FarmId: order.FarmId,
            FarmName: order.Farm?.Name ?? string.Empty,
            Price: order.Price,
            StartDate: order.OrderTrip?.StartDate ?? default,
            EndDate: order.OrderTrip?.EndDate ?? default
        );
    };

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Query, Result<PaginatedList<Response>>>
    {
        public async Task<Result<PaginatedList<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!(currentUser.User!.IsManager() || currentUser.User!.IsAdmin()))
                return Result<PaginatedList<Response>>.Fail(new ForbiddenException("Unauthorized."));

            IQueryable<Order> query = context.Orders
                .AsNoTracking()
                .Include(o => o.Farm)
                .Include(o => o.OrderTrip)
                .Include(o => o.User)
                .Where(o => o.OrderTrip.IsApproved == null);
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