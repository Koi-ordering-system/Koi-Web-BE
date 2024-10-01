using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.UseCases.UC_Orders.Commands;
using MediatR;

namespace Koi_Web_BE.Endpoints.EndpointHandlers.Orders;

public class ApproveTripOrderEndpointHandler
{
    public async static Task<IResult> Handle(ISender sender, Guid id, CancellationToken cancellationToken = default)
    {
        Result<ApproveTripOrder.Response> result = await sender.Send(new ApproveTripOrder.Command(id), cancellationToken);
        if (!result.Succeeded) return Results.BadRequest(result);
        return Results.Ok(result);
    }
}