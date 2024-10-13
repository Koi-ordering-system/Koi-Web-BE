using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Kois.Commands;

public abstract class CreateKoi
{
    public class Command : IRequest
    {
        public required Guid SpeciesId { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public bool IsMale { get; set; } = true;
        public decimal Price { get; set; } = 0;
    }
    
    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            Koi koi = new()
            {
                Name = request.Name,
                SpeciesId = request.SpeciesId,
                Description = request.Description,
                MinSize = request.MinSize,
                MaxSize = request.MaxSize,
                IsMale = request.IsMale,
                Price = request.Price,
            };
            dbContext.Kois.Add(koi);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
    
    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/kois", Handle)
                .WithTags("Kois")
                .WithMetadata(new SwaggerOperationAttribute("Create a Koi"))
                .RequireAuthorization()
                .DisableAntiforgery();
        }

        private static async Task<IResult> Handle(
            ISender sender,
            [FromBody] Command request,
            CancellationToken cancellationToken = default)
        {
            await sender.Send(request, cancellationToken);
            return Results.Created();
        }
    }
}