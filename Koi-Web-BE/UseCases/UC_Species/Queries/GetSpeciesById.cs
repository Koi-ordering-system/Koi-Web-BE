using Koi_Web_BE.Database;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.UseCases.UC_Species.Queries;

public class GetSpeciesById
{
    public record Query(Guid Id) : IRequest<Result<Response>>;

    public record Response(Guid Id, string Name)
    {
        public static Response FromEntity(Species species)
            => new(species.Id, species.Name);
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            Species? gettingSpecies = await context.Species
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Id.Equals(request.Id), cancellationToken);
            if (gettingSpecies == null) return Result<Response>.Fail(new NotFoundException("Species not found."));
            return Result<Response>.Succeed(Response.FromEntity(gettingSpecies));
        }
    }
}