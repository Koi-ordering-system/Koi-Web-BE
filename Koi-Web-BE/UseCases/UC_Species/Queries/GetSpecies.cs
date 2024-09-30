using Koi_Web_BE.Database;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.UseCases.UC_Species.Queries;

public class GetSpecies
{
    public record Query(
        string Keyword,
        int PageIndex,
        int PageSize
    ) : IRequest<Result<Response>>;

    public record Response(
        IEnumerable<SpeciesDetail> Species,
        int PageIndex,
        int PageSize,
        int TotalPages
    );

    public record SpeciesDetail(
        Guid Id,
        string Name
    )
    {
        public static SpeciesDetail FromEntity(Species species)
            => new(
                Id: species.Id,
                Name: species.Name
            );
    };

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Species> query = context.Species
                .AsNoTracking()
                .Where(s => s.Name.Trim().ToLower().Contains(request.Keyword.Trim().ToLower()));
            int total = await query.CountAsync(cancellationToken);
            IEnumerable<Species> gettingSpecies = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            return Result<Response>.Succeed(new Response(
                Species: gettingSpecies
                    .Select(SpeciesDetail.FromEntity)
                    .ToArray(),
                PageIndex: request.PageIndex,
                PageSize: request.PageSize,
                TotalPages: (int)Math.Ceiling((double)total / request.PageSize)
            ));
        }
    }
}