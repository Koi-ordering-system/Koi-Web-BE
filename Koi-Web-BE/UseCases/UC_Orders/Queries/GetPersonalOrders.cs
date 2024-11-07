using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Queries;

public class GetPersonalOrders
{
    public record Query(
        int PageIndex,
        int PageSize
    ) : IRequest<Result<Response>>;

    public record Response(
        IEnumerable<OrderDetailResponse> Items,
        int PageNumber,
        int TotalCount,
        int TotalPages
    );

    public record OrderDetailResponse(
        Guid Id,
        string UserId,
        Guid FarmId,
        string FarmName,
        decimal Price,
        bool IsPaid,
        string Status,
        KoiDetail[] Kois,
        TripDetail? Trip
    )
    {
        public static OrderDetailResponse FromEntity(Order order)
            => new(
                Id: order.Id,
                UserId: order.UserId,
                FarmId: order.FarmId,
                FarmName: order.Farm?.Name ?? string.Empty,
                Price: order.Price,
                IsPaid: order.IsPaid,
                Status: order.Status.ToString() ?? string.Empty,
                Kois: order.OrderKois.Count == 0 ? [] : order.OrderKois.Select(ok => ok.Koi).Select(k => new KoiDetail
                {
                    Id = k.Id,
                    Name = k.Name,
                    Description = k.Description,
                    MinSize = k.MinSize,
                    MaxSize = k.MaxSize,
                    Price = k.Price,
                    ImageUrls = k.Images.Select(i => i.Url).ToArray(),
                    Colors = k.Colors.Select(c => c.Name).ToArray()
                }).ToArray(),
                Trip: order.OrderTrip is null ? null : new(
                    Id: order.OrderTrip?.Id ?? default,
                    Days: order.OrderTrip?.Trip?.Days ?? 0,
                    Price: order.OrderTrip?.Trip?.Price ?? 0
                )
            );
    }

    public class KoiDetail
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public string[] ImageUrls { get; set; } = [];
        public string[] Colors { get; set; } = [];
    }

    public record TripDetail(
        Guid Id,
        int Days,
        decimal Price
    );

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
            => app.MapGet("api/orders/personal", Handle)
                .WithTags("Orders")
                .WithMetadata(new SwaggerOperationAttribute("Get Personal Orders"))
                .RequireAuthorization();

        public async static Task<IResult> Handle(ISender sender, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            Result<Response> response = await sender
                        .Send(new Query(pageIndex, pageSize), cancellationToken);
            if (!response.Succeeded) return Results.NotFound(response);
            return Results.Ok(response);
        }
    }

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Order> query = context.Orders
                .AsNoTracking()
                .AsSplitQuery()
                .Include(o => o.Farm)
                .Include(o => o.OrderKois).ThenInclude(ok => ok.Koi)
                .Include(o => o.OrderTrip).ThenInclude(o => o.Trip)
                .Where(o => o.UserId.Equals(currentUser.User!.Id ?? null))
                .Where(o => o.IsPaid);
            int count = await query.CountAsync(cancellationToken);
            IEnumerable<OrderDetailResponse> orders = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => OrderDetailResponse.FromEntity(o))
                .ToListAsync(cancellationToken);
            return Result<Response>.Succeed(new Response(
                Items: orders,
                TotalCount: count,
                PageNumber: request.PageIndex,
                TotalPages: (int)Math.Ceiling((double)count / request.PageSize)
            ));
        }
    }
}