using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Kois.Commands;

public abstract class CreateKoi
{
    public class Command : IRequest
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public IEnumerable<string> Colors { get; set; } = [];
        public IFormFileCollection KoiImages { get; set; } = new FormFileCollection();
    }

    public class Handler(IApplicationDbContext dbContext, IOutputCacheStore store, IImageService imageService) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            Guid koiId = Guid.NewGuid();
            Koi koi = new()
            {
                Id = koiId,
                Name = request.Name,
                Description = request.Description,
                MinSize = request.MinSize,
                MaxSize = request.MaxSize,
                Price = request.Price,
                Colors = [..request.Colors.Select(x => new Color
                {
                    KoiId = koiId,
                    Name = x,
                })]
            };
            dbContext.Kois.Add(koi);
            var uploadTasks = request.KoiImages.Select(async image =>
            {
                var imageUrl = await imageService.UploadImageAsync(image, image.FileName, "kois");
                return new KoiImage
                {
                    KoiId = koi.Id,
                    Url = imageUrl,
                };
            });
            var koiImages = await Task.WhenAll(uploadTasks);
            ((List<KoiImage>)koi.Images).AddRange(koiImages);

            // dbContext.KoiImages.AddRange(koiImages);

            await dbContext.SaveChangesAsync(cancellationToken);
            await store.EvictByTagAsync("Kois", cancellationToken);
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
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] decimal minSize,
            [FromForm] decimal maxSize,
            [FromForm] decimal price,
            [FromForm] IFormFileCollection koiImages,
            [FromForm] string colors,
            CancellationToken cancellationToken = default)
        {
            await sender.Send(new Command
            {
                Name = name,
                Description = description,
                Price = price,
                MaxSize = maxSize,
                MinSize = minSize,
                KoiImages = koiImages,
                Colors = colors.Split(','),
            }, cancellationToken);
            return Results.Created();
        }
    }
}