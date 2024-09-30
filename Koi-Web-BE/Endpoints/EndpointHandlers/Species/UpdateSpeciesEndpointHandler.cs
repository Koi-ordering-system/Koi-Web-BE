using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.UseCases.UC_Species.Command;
using MediatR;

namespace Koi_Web_BE.Endpoints.EndpointHandlers.Species;

public class UpdateSpeciesEndpointHandler
{
    public static async Task<IResult> Handle(ISender sender, Guid id, string name, CancellationToken cancellationToken = default)
    {
        Result<UpdateSpecies.Response> result = await sender.Send(new UpdateSpecies.Command(id, name), cancellationToken);
        if (!result.Succeeded)
            return Results.BadRequest(result.Message);
        return Results.NoContent();
    }
}