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
            int result = await context
                .Species
                .Where(x => x.Id.Equals(request.Id))
                .ExecuteDeleteAsync(cancellationToken);
            if (result == 0) return Result<Response>.Fail(new NotFoundException("Species not found."));
            return Result<Response>.Succeed(null!);
        }
    }
}