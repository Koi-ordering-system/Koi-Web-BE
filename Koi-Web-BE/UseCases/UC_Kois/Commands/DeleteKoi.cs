using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Kois.Commands;

public class DeleteKoi
{
    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/kois/{koiId:guid}", Handle)
                .WithTags("Kois")
                .WithMetadata(new SwaggerOperationAttribute("Delete Koi"))
                .RequireAuthorization();
        }

        private static async Task<IResult> Handle(
            ISender sender,
            [FromRoute] Guid koiId,
            CancellationToken cancellationToken = default)
        {
            await sender.Send(new Command(koiId), cancellationToken);
            return Results.NoContent();
        }
    }

    public record Command(Guid KoiId) : IRequest;

    public class Handler(IApplicationDbContext dbContext, IOutputCacheStore store) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            Koi? koi = dbContext.Kois.FirstOrDefault(x => x.Id == request.KoiId);
            if (koi is null)
            {
                throw new NotFoundException("Koi not found");
            }

            dbContext.Kois.Remove(koi);

            await dbContext.SaveChangesAsync(cancellationToken);
            await store.EvictByTagAsync("Kois", cancellationToken);
        }
    }
}