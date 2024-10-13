using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        public bool IsMale { get; set; } = true;
        public decimal Price { get; set; } = 0;
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
            await sender.Send(request, cancellationToken);
            return Results.Created();
        }
    }
    
    public class Command : IRequest
    {
        public Guid Id { get; set; }
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
            Koi? koi = dbContext.Kois.FirstOrDefault(x => x.Id == request.Id);
            if (koi is null)
            {
                throw new NotFoundException("Koi not found");
            }
            koi.Name = request.Name.Trim();
            koi.Description = request.Description.Trim();
            koi.MinSize = request.MinSize;
            koi.MaxSize = request.MaxSize;
            koi.IsMale = request.IsMale;
            koi.Price = request.Price;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}