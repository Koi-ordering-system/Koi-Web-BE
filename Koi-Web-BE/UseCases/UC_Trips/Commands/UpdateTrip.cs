using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Trips.Commands;
public class UpdateTrip
{
    public record UpdateTripRequest(
        Guid FarmId,
        int Days,
        decimal Price
    );
    public record Command(
        Guid Id,
        Guid FarmId,
        int Days,
        decimal Price
    ) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context.Trips
                .Where(t => t.Id.Equals(request.Id))
                .ExecuteUpdateAsync(e =>
                    e.SetProperty(t => t.FarmId, request.FarmId)
                    .SetProperty(t => t.Days, request.Days)
                    .SetProperty(t => t.Price, request.Price)
                    .SetProperty(t => t.IsApproved, (bool?)null!), cancellationToken);
            if (result == 0) return Result<Response>.Fail(new NotFoundException("Trip not found."));
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPut("api/trips/{id:guid}", Handle)
                .WithTags("Trips")
                .WithMetadata(new SwaggerOperationAttribute("Deny a Trip"))
                .RequireAuthorization();
        }

        public async static Task<IResult> Handle(Guid id, UpdateTripRequest request, ISender sender)
        {
            Result<Response> response = await sender.Send(new Command(id, request.FarmId, request.Days, request.Price), default);
            if (!response.Succeeded) return Results.BadRequest(response);
            return Results.NoContent();
        }
    }
}