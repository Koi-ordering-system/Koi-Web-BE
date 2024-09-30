using Koi_Web_BE.Models.Primitives;
using MediatR;
using static Koi_Web_BE.UseCases.UC_Species.Command.CreateSpecies;

namespace Koi_Web_BE.Endpoints.EndpointHandlers.Species;

public class CreateSpeciesEndpointHandler
{
    public static async Task<IResult> Handle(ISender sender, string name, CancellationToken cancellationToken = default)
    {
        Result<Response> result = await sender.Send(new Command(Name: name), cancellationToken);
        if (!result.Succeeded)
            return Results.BadRequest(result.Message);
        return Results.Created("", result);
    }
}