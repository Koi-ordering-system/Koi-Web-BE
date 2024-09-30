using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.UseCases.UC_Species.Queries;
using MediatR;

namespace Koi_Web_BE.Endpoints.EndpointHandlers.Species;

public class GetSpeciesByIdEndpointHandler
{
    public static async Task<IResult> Handle(ISender sender, Guid id, CancellationToken cancellationToken = default)
    {
        Result<GetSpeciesById.Response> query = await sender.Send(new GetSpeciesById.Query(id), cancellationToken);
        if (!query.Succeeded) return Results.NotFound(query);
        return Results.Ok(query);
    }
}