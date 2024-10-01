using Koi_Web_BE.Endpoints.EndpointHandlers.Orders;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.Endpoints;

public static class OrdersEndpoints
{
    public static RouteGroupBuilder DefineOrdersEndpoints(this RouteGroupBuilder app)
    {
        // GET
        app.MapGet("/personal", GetPersonalOrdersEndpointHandler.Handle)
            .WithMetadata(new SwaggerOperationAttribute("Get Personal Orders"))
            .RequireAuthorization();
        return app;
    }
}