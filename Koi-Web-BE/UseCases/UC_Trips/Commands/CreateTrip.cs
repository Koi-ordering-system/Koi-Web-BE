using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Trips.Commands;

public class CreateTrip
{
    public record CreateTripRequest(Guid farmId, int days, int price);
    public record Command(Guid FarmId, int Days, int Price) : IRequest<Result<Response>>;

    public record Response(Guid Id);
    public class Handler(IApplicationDbContext context, IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            Farm? checkingFarm = await context.Farms.AsNoTracking().SingleOrDefaultAsync(f => f.Id == request.FarmId, cancellationToken);
            if (checkingFarm is null) return Result<Response>.Fail(new NotFoundException("Farm not found."));
            Trip trip = new()
            {
                Id = Guid.NewGuid(),
                FarmId = request.FarmId,
                Days = request.Days,
                Price = request.Price,
                IsApproved = null!,
            };
            await context.Trips.AddAsync(trip, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await store.EvictByTagAsync("Trips", cancellationToken);
            return Result<Response>.Succeed(new Response(trip.Id));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("api/trips", Handle)
                .WithTags("Trips")
                .WithMetadata(new SwaggerOperationAttribute("Create a Trip"))
                .RequireAuthorization();
        }
        public static async Task<IResult> Handle(ISender sender, [FromBody] CreateTripRequest request, CancellationToken cancellationToken)
        {
            Result<Response> result = await sender.Send(new Command(request.farmId, request.days, request.price), cancellationToken);
            if (!result.Succeeded) return TypedResults.BadRequest(result);
            return TypedResults.Created(result.Message, result);
        }
    }
}