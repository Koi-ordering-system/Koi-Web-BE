using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class UpdateOrderStatus
{
    public record Command(Guid Id, OrderStatusEnum Status) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context, CurrentUser currentUser)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            if (currentUser.User is null)
                Result<Response>.Fail(new UnauthorizedException("User not found."));

            var order = await context
                .Orders.Where(o => o.Id == request.Id)
                .ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(o => o.Status, request.Status)
                            .SetProperty(o => o.UpdatedAt, DateTime.UtcNow),
                    cancellationToken
                );

            if (order == 0)
                return Result<Response>.Fail(new NotFoundException("Order not found."));

            return Result<Response>.Succeed(null);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/orders/{id}/status", Handle)
                .WithTags("Orders")
                .WithMetadata(new SwaggerOperationAttribute("Update Order Status"))
                .RequireAuthorization();
        }

        public static async Task<IResult> Handle(
            ISender sender,
            Guid id,
            OrderStatusEnum status,
            CancellationToken cancellationToken = default
        )
        {
            Result<Response> result = await sender.Send(new Command(id, status), cancellationToken);
            if (!result.Succeeded)
                return Results.BadRequest(result);
            return Results.NoContent();
        }
    }
}
