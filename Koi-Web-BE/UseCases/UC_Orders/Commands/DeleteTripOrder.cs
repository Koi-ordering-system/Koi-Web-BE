using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class DeleteTripOrder
{
    public record Command(
        Guid OrderId
    ) : IRequest<Result<Response>>;

    public record DeleteTripOrderRequest(
        Guid orderId
    );

    public record Response();

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context.OrderTrips
                .Where(ot => ot.OrderId == request.OrderId)
                .ExecuteDeleteAsync(cancellationToken);
            if (result == 0) return Result<Response>.Fail(new NotFoundException("Order not found."));
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/orders/{orderId:guid}", async (ISender sender, [AsParameters] DeleteTripOrderRequest request) =>
            {
                Result<Response> response = await sender.Send(new Command(request.orderId), default);
                if (!response.Succeeded) return Results.NotFound(response);
                return Results.Ok(response);
            })
            .WithTags("Orders")
            .WithMetadata(new SwaggerOperationAttribute("Delete Trip Order"))
            .RequireAuthorization();
        }
    }
}