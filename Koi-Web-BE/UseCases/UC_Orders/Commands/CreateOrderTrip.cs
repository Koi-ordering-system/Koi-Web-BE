using FluentValidation;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Net.payOS.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class CreateOrderTrip
{
    public record Command(
        string UserId,
        Guid TripId,
        int Quantity,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate
    ) : IRequest<Result<Response>>;

    public record Response(string PayOSUrl);

    public class Handler(IApplicationDbContext context, IPayOSServices payOSServices)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            // Check if user and trip exist
            User user =
                await context.Users.FindAsync([request.UserId], cancellationToken)
                ?? throw new NotFoundException("User not found");

            Trip trip =
                await context.Trips.FindAsync([request.TripId], cancellationToken)
                ?? throw new NotFoundException("Trip not found");

            int totalPrice = (int)(request.Quantity * trip.Price);

            Order creatingOrder =
                new()
                {
                    UserId = request.UserId,
                    FarmId = trip!.FarmId,
                    OrderTrip = new OrderTrip
                    {
                        OrderId = Guid.NewGuid(),
                        TripId = request.TripId,
                        Quantity = request.Quantity,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        Status = TripStatusEnum.Pending
                    },
                    Price = totalPrice,
                    IsPaid = false,
                };

            await context.Orders.AddAsync(creatingOrder, cancellationToken);

            // Create payOS order
            List<ItemData> itemDatas =
            [
                new ItemData(
                    name: "Number of people",
                    quantity: request.Quantity,
                    price: (int)trip.Price
                )
            ];

            CreatePaymentResult payOSUrl = await payOSServices.CreateOrderAsync(
                totalPrice,
                itemDatas
            );

            // Update order with payOS order code
            creatingOrder.PayOSOrderCode = payOSUrl.orderCode;

            await context.SaveChangesAsync(cancellationToken);

            return Result<Response>.Succeed(new Response(payOSUrl.checkoutUrl));
        }
    }

    public record CreateTripOrderRequest(
        string UserId,
        Guid TripId,
        int Quantity,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate
    );

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/orders/trip/create", Handle)
                .WithTags("Orders")
                .WithMetadata(new SwaggerOperationAttribute("Create an order for a trip"))
                .RequireAuthorization();
        }

        public static async Task<IResult> Handle(
            CreateTripOrderRequest request,
            IApplicationDbContext context,
            IPayOSServices payOSServices
        )
        {
            Result<Response> response = await new Handler(context, payOSServices).Handle(
                new Command(
                    request.UserId,
                    request.TripId,
                    request.Quantity,
                    request.StartDate,
                    request.EndDate
                ),
                default
            );

            return !response.Succeeded
                ? Results.BadRequest(response)
                : Results.Created(response.Message, response);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required");
            RuleFor(x => x.EndDate)
                .NotEmpty()
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be greater than start date");
        }
    }
}
