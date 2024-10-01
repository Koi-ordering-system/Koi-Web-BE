using Koi_Web_BE.Database;
using Koi_Web_BE.Exceptions;
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
            int result = await context.Species
                .Where(s => s.Id.Equals(request.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Name, request.Name), cancellationToken);
            if (result == 0) return Result<Response>.Fail(new NotFoundException("Species not found."));
            return Result<Response>.Succeed(null!);
        }
    }
}