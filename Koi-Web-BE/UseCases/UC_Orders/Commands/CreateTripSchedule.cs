using FluentValidation;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class CreateTripSchedule
{
    public record CreateTripRequest(
        string UserId,
        Guid TripId,
        decimal Price,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate
    );

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/trips", async (ISender sender, CreateTripRequest request) =>
            {
                Result<Response> result = await sender.Send(new Command(request.UserId,
                                                                    request.TripId,
                                                                    request.Price,
                                                                    request.StartDate,
                                                                    request.EndDate), default);
                if (!result.Succeeded)
                    return Results.BadRequest(result);
                return Results.Created("", result);
            }).WithTags("Orders")
                .WithMetadata(new SwaggerOperationAttribute("Create Trip Schedule"))
                .RequireAuthorization();
        }
    }

    public record Command(
        string UserId,
        Guid TripId,
        decimal Price,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate
    ) : IRequest<Result<Response>>;

    public record Response(Guid Id);

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (currentUser.User is null)
                return Result<Response>.Fail(new UnauthorizedException("User not found."));

            if (currentUser.User.Role == RoleEnum.Customer)
                return Result<Response>.Fail(new ForbiddenException("You are not allowed to create trip schedules."));

            Trip? trip = await context.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TripId, cancellationToken);

            if (trip is null)
                return Result<Response>.Fail(new NotFoundException("Trip not found."));

            User? user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user is null)
                return Result<Response>.Fail(new NotFoundException("User not found."));

            Order creatingOrder = new()
            {
                UserId = request.UserId,
                FarmId = trip!.FarmId,
                OrderTrip = new OrderTrip
                {
                    OrderId = Guid.NewGuid(),
                    TripId = request.TripId,
                    Status = TripStatusEnum.Pending,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                },
                Price = request.Price,
                IsPaid = false,
            };
            await context.Orders.AddAsync(creatingOrder, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return Result<Response>.Succeed(new Response(creatingOrder.Id));
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate);
        }
    }
}