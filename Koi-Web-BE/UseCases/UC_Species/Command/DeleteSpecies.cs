using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

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

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/species/{id}", Handle)
            .WithTags("Species")
            .WithMetadata(new SwaggerOperationAttribute("Delete a Species"))
            .RequireAuthorization();
        }
        public static async Task<IResult> Handle(ISender sender, Guid id, CancellationToken cancellationToken = default)
        {
            Result<Response> result = await sender.Send(new Command(id), cancellationToken);
            if (!result.Succeeded)
                return Results.BadRequest(result);
            return Results.NoContent();
        }
    }
}