using Koi_Web_BE.Database;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;

namespace Koi_Web_BE.UseCases.UC_Species.Command;

public class CreateSpecies
{
    public record Command(string Name) : IRequest<Result<Response>>;

    public record Response(
        Guid Id,
        string Name
    )
    {
        public static Response FromEntity(Species species)
            => new(
                Id: species.Id,
                Name: species.Name
            );
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // initialize
            Species addingSpecies = new()
            {
                Name = request.Name,
            };
            // add to db
            context.Species.Add(addingSpecies);
            await context.SaveChangesAsync(cancellationToken);
            // return result
            return Result<Response>.Succeed(Response.FromEntity(addingSpecies));
        }
    }
}