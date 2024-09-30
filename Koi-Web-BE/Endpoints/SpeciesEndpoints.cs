using Koi_Web_BE.Endpoints.EndpointHandlers.Species;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.Endpoints;

public static class SpeciesEndpoints
{
    public static RouteGroupBuilder DefineSpeciesEndpoints(this RouteGroupBuilder app)
    {
        // GET
        app.MapGet("{id}", GetSpeciesByIdEndpointHandler.Handle)
            .WithMetadata(new SwaggerOperationAttribute("Get a Species"))
            .RequireAuthorization();
        // POST
        app.MapPost("", CreateSpeciesEndpointHandler.Handle)
            .WithMetadata(new SwaggerOperationAttribute("Create a Species"))
            .RequireAuthorization();
        // DELETE
        app.MapDelete("{id}", DeleteSpeciesEndpointHandler.Handle)
            .WithMetadata(new SwaggerOperationAttribute("Delete a Species"))
            .RequireAuthorization();
        // PUT
        app.MapPut("{id}", UpdateSpeciesEndpointHandler.Handle)
            .WithMetadata(new SwaggerOperationAttribute("Update a Species"))
            .RequireAuthorization();
        return app;
    }
}