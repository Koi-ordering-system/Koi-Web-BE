using Koi_Web_BE.Database;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.UseCases.UC_Species.Command;

public class DeleteSpecies
{
    public record Command(Guid Id) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            Species? deletingSpecies = await context.Species.SingleOrDefaultAsync(s => s.Id.Equals(request.Id), cancellationToken);
            if (deletingSpecies is null) return Result<Response>.Fail(new NotFoundException("Species not found"));
            context.Species.Remove(deletingSpecies);
            await context.SaveChangesAsync(cancellationToken);
            return Result<Response>.Succeed(new Response());
        }
    }
}