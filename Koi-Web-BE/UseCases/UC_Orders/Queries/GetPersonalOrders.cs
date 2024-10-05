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
        IEnumerable<OrderDetailResponse> Orders,
        int PageIndex,
        int PageSize,
        int TotalPages
    );

    public record OrderDetailResponse(
        Guid Id,
        string UserId,
        Guid FarmId,
        string FarmName,
        decimal Price,
        bool IsPaid,
        string Status
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
                Status: order.Status.ToString() ?? string.Empty
            );
    }

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
                .Include(o => o.Farm)
                .Where(o => o.UserId.Equals(currentUser.User!.Id ?? null));
            int count = await query.CountAsync(cancellationToken);
            IEnumerable<OrderDetailResponse> orders = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => OrderDetailResponse.FromEntity(o))
                .ToListAsync(cancellationToken);
            return Result<Response>.Succeed(new Response(
                Orders: orders,
                PageIndex: request.PageIndex,
                PageSize: request.PageSize,
                TotalPages: (int)Math.Ceiling((double)count / request.PageSize)
            ));
        }
    }
}