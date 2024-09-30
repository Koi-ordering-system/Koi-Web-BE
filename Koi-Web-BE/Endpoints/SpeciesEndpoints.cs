using Koi_Web_BE.Endpoints.EndpointHandlers.Species;

namespace Koi_Web_BE.Endpoints;

public static class SpeciesEndpoints
{
    public static RouteGroupBuilder DefineSpeciesEndpoints(this RouteGroupBuilder app)
    {
        app.MapPost("", CreateSpeciesEndpointHandler.Handle);
        app.MapDelete("{Id}", DeleteSpeciesEndpointHandler.Handle);
        return app;
    }
}