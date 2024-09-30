using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.UseCases.UC_Species.Command;
using MediatR;

namespace Koi_Web_BE.Endpoints.EndpointHandlers.Species;

public class DeleteSpeciesEndpointHandler
{
    public static async Task<IResult> Handle(ISender sender, Guid id, CancellationToken cancellationToken = default)
    {
        Result<DeleteSpecies.Response> result = await sender.Send(new DeleteSpecies.Command(id), cancellationToken);
        if (!result.Succeeded)
            return Results.BadRequest(result);
        return Results.NoContent();
    }
}