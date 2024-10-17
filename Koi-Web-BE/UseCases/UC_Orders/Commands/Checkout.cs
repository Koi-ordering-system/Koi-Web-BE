using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class Checkout
{
    public record Command() : IRequest<Result<Response>>;

    public record Response(string CheckoutUrl);

    public class Handler(
        IApplicationDbContext context,
        CurrentUser currentUser,
        IPayOSServices payOSServices
    ) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            Cart? cart = await context
                .Carts.AsSplitQuery()
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FarmKoi)
                .ThenInclude(fk => fk.Koi)
                .Where(c => c.UserId == currentUser.User!.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return Result<Response>.Fail(new NotFoundException("Cart not found."));

            if (cart.CartItems.Count == 0)
                return Result<Response>.Fail(new NotFoundException("Cart is empty."));

            int totalAmount = (int)cart.CartItems.Sum(ci => ci.FarmKoi.Koi.Price * ci.Quantity);

            // Add each product from CartItems to the productList
            List<ItemData> productList =
            [
                .. cart
                    .CartItems.AsQueryable()
                    .AsNoTracking()
                    .Select(c => new ItemData(
                        c.FarmKoi.Koi.Name,
                        c.Quantity,
                        (int)c.FarmKoi.Koi.Price
                    ))
            ];

            CreatePaymentResult response = await payOSServices.CreateOrderAsync(
                totalAmount,
                productList
            );

            // Create order
            Order order =
                new()
                {
                    UserId = currentUser.User!.Id,
                    FarmId = cart.CartItems.First().FarmKoi.FarmId,
                    PayOSOrderCode = response.orderCode,
                    Price = totalAmount,
                    IsPaid = false,
                    Status = OrderStatusEnum.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

            // Create OrderKoi items
            var orderKois = cart
                .CartItems.Select(ci => new OrderKoi
                {
                    OrderId = order.Id,
                    KoiId = ci.FarmKoi.KoiId,
                    Quantity = ci.Quantity
                })
                .ToList();

            await context.Orders.AddAsync(order, cancellationToken);
            await context.OrderKois.AddRangeAsync(orderKois, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return Result<Response>.Succeed(new Response(response.checkoutUrl));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("api/checkout", Handle)
                .WithTags("Orders")
                .WithMetadata(new SwaggerOperationAttribute("Checkout"))
                .RequireAuthorization();
        }

        public static async Task<IResult> Handle(
            ISender sender,
            CancellationToken cancellationToken = default
        )
        {
            Result<Response> result = await sender.Send(new Command(), cancellationToken);

            if (!result.Succeeded)
                return Results.BadRequest(result);

            return Results.Created("", result);
        }
    }
}
