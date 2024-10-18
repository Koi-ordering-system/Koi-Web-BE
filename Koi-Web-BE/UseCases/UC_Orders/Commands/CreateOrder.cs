// using System.ComponentModel.DataAnnotations;
// using FluentValidation;
// using Koi_Web_BE.Database;
// using Koi_Web_BE.Endpoints.Internal;
// using Koi_Web_BE.Exceptions;
// using Koi_Web_BE.Models.Entities;
// using Koi_Web_BE.Models.Primitives;
// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using Swashbuckle.AspNetCore.Annotations;

// namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

// public class CreateOrder
// {
//     public record Command(Guid FarmKoiId,
//         string Color,
//         decimal Size,
//         int Quantity = 1) : IRequest<Result<Response>>;

//     public record Response();

//     public class Handler(IApplicationDbContext context, CurrentUser currentUser)
//         : IRequestHandler<Command, Result<Response>>
//     {
//         public async Task<Result<Response>> Handle(
//             Command request,
//             CancellationToken cancellationToken
//         )
//         {
//             if (currentUser.User is null)
//                 Result<Response>.Fail(new UnauthorizedException("User not found."));

//             // Check if koi belong to the farm exists
//             FarmKoi? koi = await context
//                 .FarmKois.AsNoTracking()
//                 .FirstOrDefaultAsync(k => k.Id == request.FarmKoiId, cancellationToken);

//             if (koi is null)
//                 return Result<Response>.Fail(new NotFoundException("Koi not found."));

//             if (koi.Quantity < request.Quantity)
//                 return Result<Response>.Fail(new BadRequestException("Not enough quantity."));

//             Cart? cart = await context
//                 .Carts
//                 .AsNoTracking()
//                 .Include(c => c.CartItems)
//                 .FirstOrDefaultAsync(c => c.UserId == currentUser.User!.Id, cancellationToken);

//             if (cart is null)
//                 return Result<Response>.Fail(new NotFoundException("Cart not found."));

//             // Check if the item is already in the cart
//             CartItem? existingCartItem = cart.CartItems.FirstOrDefault(ci =>
//                 ci.FarmKoiId == request.FarmKoiId
//             );

//             if (existingCartItem is not null)
//             {
//                 existingCartItem.Quantity += request.Quantity;
//                 context.CartItems.Update(existingCartItem);
//             }
//             else
//             {
//                 CartItem newCartItem =
//                     new()
//                     {
//                         CartId = cart.Id,
//                         FarmKoiId = request.FarmKoiId,
//                         Quantity = request.Quantity,
//                         Color = request.Color,
//                         Size = request.Size
//                     };

//                 await context.CartItems.AddAsync(newCartItem, cancellationToken);
//             }

//             await context.SaveChangesAsync(cancellationToken);

//             return Result<Response>.Succeed(null);
//         }
//     }

//     public class Endpoint : IEndpoints
//     {
//         public static void DefineEndpoints(IEndpointRouteBuilder app)
//         {
//             app.MapPost("api/carts", Handle)
//                 .WithTags("Carts")
//                 .WithMetadata(new SwaggerOperationAttribute("Add Product to Cart"))
//                 .RequireAuthorization();
//         }

//         public record AddToCartRequest(Guid FarmKoiId,
//                                         string Color,
//                                         decimal Size,
//                                         [Range(1, 50)]
//                                         int Quantity = 1);

//         public static async Task<IResult> Handle(
//             ISender sender,
//             AddToCartRequest request,
//             CancellationToken cancellationToken = default
//         )
//         {
//             Result<Response> result = await sender.Send(
//                 new Command(request.FarmKoiId, request.Color, request.Size, request.Quantity),
//                 cancellationToken
//             );

//             if (!result.Succeeded)
//                 return Results.BadRequest(result);

//             return Results.Created("", result);
//         }
//     }

//     public class Validator : AbstractValidator<Command>
//     {
//         public Validator()
//         {
//             RuleFor(x => x.FarmKoiId).NotEmpty().WithMessage("FarmKoiId is required.");
//             RuleFor(x => x.Quantity)
//                 .InclusiveBetween(1, 50)
//                 .WithMessage("Quantity must be between 1 and 50.");
//         }
//     }
// }
