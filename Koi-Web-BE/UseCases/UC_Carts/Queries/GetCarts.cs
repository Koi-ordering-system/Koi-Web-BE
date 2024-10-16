using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Carts.Queries;

public class GetCarts
{
    public class Query : IRequest<Result<Response>>;

    public record Response(
        IEnumerable<ResponseItem> Carts,
        decimal Total
    );

    public record ResponseItem(
        Guid KoiId,
        string Name,
        string FarmName,
        int Quantity,
        decimal Price,
        decimal Total,
        string ImageUrl
    )
    {
        public static ResponseItem FromEntity(CartItem cart)
            => new(
                KoiId: cart.FarmKoi.KoiId,
                Name: cart.FarmKoi.Koi.Name,
                FarmName: cart.FarmKoi.Farm.Name,
                Quantity: cart.Quantity,
                Price: cart.FarmKoi.Koi.Price,
                Total: cart.Quantity * cart.FarmKoi.Koi.Price,
                ImageUrl: cart.FarmKoi.Koi.Images.FirstOrDefault()?.Url ?? string.Empty);
    };

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            Response? gettingCarts = await context.Carts
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.CartItems).ThenInclude(c => c.FarmKoi)
                .Include(c => c.CartItems).ThenInclude(c => c.FarmKoi).ThenInclude(c => c.Koi)
                .Include(c => c.CartItems).ThenInclude(c => c.FarmKoi).ThenInclude(c => c.Koi).ThenInclude(c => c.Images)
                .Include(c => c.CartItems).ThenInclude(c => c.FarmKoi).ThenInclude(c => c.Farm)
                .Where(c => c.UserId.Equals(currentUser.User!.Id))
                .Select(c => new Response(
                    c.CartItems
                        .Select(ci => ResponseItem.FromEntity(ci))
                        .ToList(),
                    c.CartItems.Sum(ci => ci.Quantity * ci.FarmKoi.Koi.Price)
                )).FirstOrDefaultAsync(cancellationToken);

            if (gettingCarts is null)
                return Result<Response>.Fail(new NotFoundException("Carts not found."));

            return Result<Response>.Succeed(gettingCarts);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/carts", async (ISender sender) =>
            {
                Result<Response> result = await sender.Send(new Query(), default);
                return Results.Ok(result);
            })
                .WithTags("Carts")
                .WithMetadata(new SwaggerOperationAttribute("Get all Carts"))
                .RequireAuthorization();
        }
    }
}