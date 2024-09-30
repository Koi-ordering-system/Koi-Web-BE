using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.UseCases.UC_Species.Queries;
using MediatR;

namespace Koi_Web_BE.Endpoints.EndpointHandlers.Species;

public class GetSpeciesEndpointHandler
{
    public static async Task<IResult> Handle(ISender sender, string Keyword = "", int PageIndex = 1, int PageSize = 10, CancellationToken cancellationToken = default)
    {
        Result<GetSpecies.Response> response = await sender.Send(new GetSpecies.Query(Keyword, PageIndex, PageSize), cancellationToken);
        if (!response.Succeeded) return Results.NotFound(response);
        return Results.Ok(response);
    }
}