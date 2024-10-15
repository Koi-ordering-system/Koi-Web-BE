using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Queries;

public class GetDeliveredOrder
{
    public record Query(int PageIndex, int PageSize) : IRequest<Result<Response>>;

    public record Response(
        IEnumerable<OrderDetailResponse> Orders,
        int PageIndex,
        int PageSize,
        int TotalPages
    );

    public record OrderDetailResponse(
        Guid Id,
        string UserId,
        string Username,
        Guid FarmId,
        string FarmName,
        decimal Price,
        bool IsPaid,
        string Status,
        DateTimeOffset CreatedAt
    )
    {
        public static OrderDetailResponse FromEntity(Order order) =>
            new(
                Id: order.Id,
                UserId: order.UserId,
                Username: order.User?.Username ?? string.Empty,
                FarmId: order.FarmId,
                FarmName: order.Farm?.Name ?? string.Empty,
                Price: order.Price,
                IsPaid: order.IsPaid,
                Status: order.Status.ToString() ?? string.Empty,
                CreatedAt: order.CreatedAt
            );
    }

    public class Handler(IApplicationDbContext context, CurrentUser currentUser)
        : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            IQueryable<Order> query = context
                .Orders.AsNoTracking()
                .Include(o => o.Farm)
                .Where(o => o.UserId.Equals(currentUser.User!.Id ?? null))
                .Where(o => o.Status == OrderStatusEnum.Completed);

            int count = await query.CountAsync(cancellationToken);

            IEnumerable<OrderDetailResponse> orders = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => OrderDetailResponse.FromEntity(o))
                .ToListAsync(cancellationToken);

            return Result<Response>.Succeed(
                new Response(
                    Orders: orders,
                    PageIndex: request.PageIndex,
                    PageSize: request.PageSize,
                    TotalPages: (int)Math.Ceiling((double)count / request.PageSize)
                )
            );
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app) =>
            app.MapGet("api/orders/delivered", Handle)
                .WithTags("Orders")
                .WithMetadata(new SwaggerOperationAttribute("Get delivered order"))
                .RequireAuthorization();

        public static async Task<IResult> Handle(
            ISender sender,
            CancellationToken cancellationToken,
            int pageIndex = 1,
            int pageSize = 10
        )
        {
            Result<Response> response = await sender.Send(
                new Query(pageIndex, pageSize),
                cancellationToken
            );

            if (!response.Succeeded)
                return Results.NotFound(response);

            return Results.Ok(response);
        }
    }
}
