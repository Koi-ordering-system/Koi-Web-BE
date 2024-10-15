using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Carts.Commands;

public class AddFishToCart
{
    public record Command(
        Guid KoiId,
        Guid FarmId
    ) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // check if current user is null
            if (currentUser.User is null)
                return Result<Response>.Fail(new UnauthorizedException("User not found."));
            // check if fish koi is existed
            FarmKoi? farmKoi = await context.FarmKois
                .AsNoTracking()
                .Where(f => f.KoiId.Equals(request.KoiId) && f.FarmId.Equals(request.FarmId))
                .SingleOrDefaultAsync(cancellationToken);
            if (farmKoi is null)
                return Result<Response>.Fail(new NotFoundException("Farm does not have this fish."));
            // check if cart is existed
            Cart? cart = await context.Carts
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.CartItems).ThenInclude(c => c.FarmKoi)
                .Where(c => c.UserId.Equals(currentUser.User.Id))
                .SingleOrDefaultAsync(cancellationToken);
            if (cart is null)
                return Result<Response>.Fail(new NotFoundException("Cart not found."));
            // check if fish is already in cart
            if (cart.CartItems.Any(ci => ci.FarmKoiId.Equals(farmKoi.Id)))
            {
                await context.CartItems
                    .Where(ci => ci.FarmKoiId.Equals(farmKoi.Id))
                    .Where(ci => ci.CartId.Equals(cart.Id))
                    .ExecuteUpdateAsync(c => c.SetProperty(ci => ci.Quantity, ci => ci.Quantity + 1), cancellationToken);
            }
            else
            {
                context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    FarmKoiId = farmKoi.Id,
                });
                await context.SaveChangesAsync(cancellationToken);
            }
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("api/carts/{koiId:guid}/{farmId:guid}", async (ISender sender, Guid farmId, Guid koiId) =>
            {
                Result<Response> result = await sender.Send(new Command(koiId, farmId), default);
                if (!result.Succeeded)
                    return Results.BadRequest(result);
                return Results.Created();
            })
                .WithTags("Carts")
                .WithMetadata(new SwaggerOperationAttribute("Add fish to cart"))
                .RequireAuthorization();
        }
    }
}