using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class ProcessPaymentWebhook
{
    public record Command(WebhookType WebhookType) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            WebhookData webhookData = request.WebhookType.data;

            int result = await context
                .Orders.AsSplitQuery()
                .Include(o => o.OrderKois)
                .Include(o => o.User)
                .Where(o => o.PayOSOrderCode == webhookData.orderCode)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.IsPaid, true), cancellationToken);

            if (result == 0)
                return Result<Response>.Fail(new NotFoundException("Order not found."));
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("api/webhook", Handle)
                .WithTags("Orders")
                .WithMetadata(
                    new SwaggerOperationAttribute(
                        "Handle payment webhook, called by PayOS to notify payment status"
                    )
                );
        }

        public static async Task<IResult> Handle(
            ISender sender,
            [FromBody] WebhookType webhookType,
            CancellationToken cancellationToken = default
        )
        {
            Result<Response> result = await sender.Send(
                new Command(webhookType),
                cancellationToken
            );

            if (!result.Succeeded)
                return Results.BadRequest(result);

            return Results.Created("", null);
        }
    }
}
