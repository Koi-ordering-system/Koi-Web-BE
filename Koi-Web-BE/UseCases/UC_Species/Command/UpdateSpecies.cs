using Koi_Web_BE.Database;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.UseCases.UC_Species.Command;

public class UpdateSpecies
{
    public record Command(Guid Id, string Name) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            Species? updatingSpecies = await context.Species
                .SingleOrDefaultAsync(s => s.Id.Equals(request.Id), cancellationToken);
            if (updatingSpecies is null) return Result<Response>.Fail(new NotFoundException("Species not found."));

            updatingSpecies.Name = request.Name;

            await context.SaveChangesAsync(cancellationToken);
            return Result<Response>.Succeed(new Response());
        }
    }
}