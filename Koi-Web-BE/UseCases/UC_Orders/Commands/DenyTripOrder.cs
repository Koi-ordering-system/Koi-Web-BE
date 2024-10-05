using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class DenyTripOrder
{
    public record Response();

    public record Command(Guid Id) : IRequest<Result<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context.OrderTrips
                .Where(ot => ot.Id.Equals(request.Id))
                .ExecuteUpdateAsync(e => e.SetProperty(ot => ot.IsApproved, false), cancellationToken);
            if (result == 0) return Result<Response>.Fail(new NotFoundException("Order not found."));
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/orders/deny/{id}", Handle)
            .WithTags("Orders")
            .WithMetadata(new SwaggerOperationAttribute("Approve Trip Order"))
            .RequireAuthorization();
        }

        public async static Task<IResult> Handle(ISender sender, Guid id, CancellationToken cancellationToken = default)
        {
            Result<Response> result = await sender.Send(new Command(id), cancellationToken);
            if (!result.Succeeded) return Results.BadRequest(result);
            return Results.NoContent();
        }
    }
}