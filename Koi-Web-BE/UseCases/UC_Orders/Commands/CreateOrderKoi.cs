using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class CreateOrderKoi
{
    public record CreateOrderKoiRequest(
        string UserId,
        Guid FarmId,
        KoiDetails[] Kois,
        decimal? PrePaidPrice
    );
    public record Command(
        string UserId,
        Guid FarmId,
        KoiDetails[] Kois,
        decimal? PrePaidPrice
    ) : IRequest<Result<Response>>;

    public record KoiDetails(
        Guid KoiId,
        int Quantity,
        Guid? Color,
        decimal? MinSize,
        decimal? MaxSize
    );

    public record Response(
        string payOSUrl
    );

    public record CheckingFarmDbResponse(
        Guid Id,
        Koi Koi,
        int Quantity,
        decimal MinSize,
        decimal MaxSize
    )
    {
        public static CheckingFarmDbResponse From(FarmKoi farmKoi) => new(farmKoi.Id, farmKoi.Koi, farmKoi.Quantity, farmKoi.Koi.MinSize, farmKoi.Koi.MaxSize);
    };


    public class Handler(
        IApplicationDbContext context,
        IPayOSServices payOSServices
    ) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            IEnumerable<CheckingFarmDbResponse> checkingFarmKois = await context.FarmKois
                .AsNoTracking()
                .Include(fk => fk.Koi)
                .Where(fk => request.Kois.Select(k => k.KoiId).Contains(fk.KoiId))
                .Where(fk => fk.FarmId == request.FarmId)
                .Select(c => CheckingFarmDbResponse.From(c))
                .ToListAsync(cancellationToken);

            if (checkingFarmKois.Count() != request.Kois.Length)
                return Result<Response>.Fail(new NotFoundException("Some Kois not found."));

            foreach (KoiDetails koi in request.Kois)
            {
                CheckingFarmDbResponse? farmKoi = checkingFarmKois.FirstOrDefault(fk => fk.Koi.Id == koi.KoiId);
                if (farmKoi is null)
                    return Result<Response>.Fail(new NotFoundException("Some Kois not found."));

                if (koi.MinSize is not null && koi.MaxSize is not null && koi.MinSize > koi.MaxSize)
                    return Result<Response>.Fail(new BadRequestException("MinSize must be less than MaxSize."));

                if (koi.MinSize is not null && farmKoi.MinSize > koi.MinSize)
                    return Result<Response>.Fail(new BadRequestException("Requesting MinSize is too small comparing to farmKoi Min size."));

                if (koi.MaxSize is not null && farmKoi.MaxSize < koi.MaxSize)
                    return Result<Response>.Fail(new BadRequestException("MaxSize must be greater than MinSize."));
            }

            Order order = new()
            {
                UserId = request.UserId,
                FarmId = request.FarmId,
                Price = request.Kois.Sum(k => k.Quantity * checkingFarmKois.First(fk => fk.Koi.Id == k.KoiId).Koi.Price),
                PrePaidPrice = request.PrePaidPrice ?? 0,
                IsPaid = false
            };

            List<ItemData> itemDatas = request.Kois
                .Select(k => new ItemData(
                    checkingFarmKois.First(fk => fk.Koi.Id == k.KoiId).Koi.Name,
                    k.Quantity,
                    (int)Math.Ceiling(checkingFarmKois.First(fk => fk.Koi.Id == k.KoiId).Koi.Price)
                ))
                .ToList();
            // Create PayOS Transactions
            CreatePaymentResult payOSUrl = await payOSServices.CreateOrderAsync((int)Math.Ceiling(order.Price), itemDatas);
            // add to db
            order.PayOSOrderCode = payOSUrl.orderCode;
            await context.Orders.AddAsync(order, cancellationToken);

            if (payOSUrl.checkoutUrl is null)
                return Result<Response>.Fail(new BadRequestException("Failed to create order."));
            await context.SaveChangesAsync(cancellationToken);
            return Result<Response>.Succeed(new Response(payOSUrl.checkoutUrl));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/orders/koi", Handle)
                .WithTags("Orders")
                .WithMetadata(new SwaggerOperationAttribute("Create an order"))
                .RequireAuthorization();
        }

        public async static Task<IResult> Handle(
            CreateOrderKoiRequest request,
            IApplicationDbContext context,
            IPayOSServices payOSServices
        )
        {
            Result<Response> response = await new Handler(context, payOSServices).Handle(new Command(request.UserId, request.FarmId, request.Kois, request.PrePaidPrice), default);
            if (!response.Succeeded) return Results.BadRequest(response);
            return Results.Created(response.Message, response);
        }
    }
}
