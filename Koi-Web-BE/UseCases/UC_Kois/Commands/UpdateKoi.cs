using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Kois.Commands;

public class UpdateKoi
{
    public class Request
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public string Colors { get; set; } = string.Empty;
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/kois/{koiId:guid}", Handle)
                .WithTags("Kois")
                .WithMetadata(new SwaggerOperationAttribute("Update Koi"))
                .RequireAuthorization()
                .DisableAntiforgery();
        }

        private static async Task<IResult> Handle(
            ISender sender,
            [FromRoute] Guid koiId,
            [FromBody] Request request,
            CancellationToken cancellationToken = default)
        {
            await sender.Send(new Command()
            {
                Id = koiId,
                Name = request.Name,
                Description = request.Description,
                MinSize = request.MinSize,
                MaxSize = request.MaxSize,
                Price = request.Price,
                Colors = [.. request.Colors.Split(',')],
            }, cancellationToken);
            return Results.NoContent();
        }
    }

    public class Command : IRequest
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public IEnumerable<string> Colors { get; set; } = [];
    }

    public class Handler(IApplicationDbContext dbContext, IOutputCacheStore store) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            Koi? koi = dbContext.Kois.Include(x => x.Colors).FirstOrDefault(x => x.Id == request.Id);
            if (koi is null)
                throw new NotFoundException("Koi not found");
            koi.Name = request.Name.Trim();
            koi.Description = request.Description.Trim();
            koi.MinSize = request.MinSize;
            koi.MaxSize = request.MaxSize;
            koi.Price = request.Price;
            await dbContext.Colors.Where(x => x.KoiId == request.Id).ExecuteDeleteAsync(cancellationToken);
            dbContext.Colors.AddRange(request.Colors.Select(x => new Color
            {
                KoiId = koi.Id,
                Name = x,
            }));
            await dbContext.SaveChangesAsync(cancellationToken);
            await store.EvictByTagAsync("Kois", cancellationToken);
        }
    }
}