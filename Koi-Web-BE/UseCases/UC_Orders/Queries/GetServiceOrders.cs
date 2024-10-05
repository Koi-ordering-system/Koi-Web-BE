using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Database;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Koi_Web_BE.Endpoints.Internal;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Queries;

public class GetServiceOrders
{
    public record Query(
        int PageIndex,
        int PageSize
    ) : IRequest<Result<Response>>;

    public record Response(
        int PageIndex,
        int PageSize,
        int TotalPages,
        IEnumerable<OrderDetailResponse> Orders
    );

    public record OrderDetailResponse(
        Guid Id,
        Guid OrderTripId,
        string UserId,
        Guid FarmId,
        string FarmName,
        decimal Price,
        bool IsPaid,
        string Status,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate,
        bool? IsApproved,
        TripStatusEnum TripStatusEnum
    )
    {
        public static OrderDetailResponse FromEntity(Order order)
            => new(
                Id: order.Id,
                OrderTripId: order.OrderTrip?.Id ?? default,
                UserId: order.UserId,
                FarmId: order.FarmId,
                FarmName: order.Farm?.Name ?? string.Empty,
                Price: order.Price,
                IsPaid: order.IsPaid,
                Status: order.Status?.ToString() ?? string.Empty,
                StartDate: order.OrderTrip?.StartDate ?? default,
                EndDate: order.OrderTrip?.EndDate ?? default,
                IsApproved: order.OrderTrip?.IsApproved,
                TripStatusEnum: order.OrderTrip?.Status ?? default
            );
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Order> query = context.Orders
                                            .AsNoTracking()
                                            .Include(o => o.Farm)
                                            .Include(o => o.OrderTrip);
            int count = await query.CountAsync(cancellationToken);
            IEnumerable<OrderDetailResponse> orders = await query.Skip((request.PageIndex - 1) * request.PageSize)
                                                                .Take(request.PageSize)
                                                                .OrderByDescending(o => o.CreatedAt)
                                                                .Select(o => OrderDetailResponse.FromEntity(o))
                                                                .ToListAsync(cancellationToken);
            return Result<Response>.Succeed(new(
                PageIndex: request.PageIndex,
                PageSize: request.PageSize,
                TotalPages: (int)Math.Ceiling((double)count / request.PageSize),
                Orders: orders
            ));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("api/orders/service", Handle)
            .WithTags("Orders")
            .WithMetadata(new SwaggerOperationAttribute("Get Service Orders"))
            .RequireAuthorization();
        }

        public static async Task<IResult> Handle(ISender sender, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            Result<Response> response = await sender.Send(new Query(pageIndex, pageSize), cancellationToken);
            if (!response.Succeeded) return Results.NotFound(response);
            return Results.Ok(response);
        }
    }

}