using Koi_Web_BE.Database;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.UseCases.UC_Species.Command;

public class DeleteSpecies
{
    public record Command(Guid Id) : IRequest<Result<DeleteSpecies>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<DeleteSpecies>>
    {
        public async Task<Result<DeleteSpecies>> Handle(Command request, CancellationToken cancellationToken)
        {
            Species? deletingSpecies = await context.Species.SingleOrDefaultAsync(s => s.Id.Equals(request.Id), cancellationToken);
            if (deletingSpecies is null) return Result<DeleteSpecies>.Fail(new NotFoundException("Species not found"));
            context.Species.Remove(deletingSpecies);
            await context.SaveChangesAsync(cancellationToken);
            return Result<DeleteSpecies>.Succeed(null!);
        }
    }
}