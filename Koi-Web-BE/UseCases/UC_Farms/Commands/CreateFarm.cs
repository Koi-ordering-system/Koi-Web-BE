using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Farms.Commands;

public class CreateFarm
{
    public record Command(
        string Name,
        string Owner,
        string Address,
        string Description,
        decimal Rating,
        IFormFileCollection FarmImages
    ) : IRequest<Result<Response>>;

    public record Response(
        Guid Id,
        string Name
    )
    {
        public static Response FromEntity(Farm farm)
            => new(
                Id: farm.Id,
                Name: farm.Name
            );
    }

    public class Handler(IApplicationDbContext context, IImageService imageService) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Initialize the farm entity
            Farm addingFarm = new()
            {
                Name = request.Name,
                Owner = request.Owner,
                Address = request.Address,
                Description = request.Description,
                Rating = request.Rating
            };
            // Upload images to Cloudinary and add URLs to the farm
            var uploadTasks = request.FarmImages.Select(async image =>
            {
                var imageUrl = await imageService.UploadImageAsync(image, image.FileName, "farms");
                return new FarmImage
                {
                    FarmId = addingFarm.Id,
                    Url = imageUrl
                };
            });
            var farmImages = await Task.WhenAll(uploadTasks);
            ((List<FarmImage>)addingFarm.FarmImages).AddRange(farmImages);
            // Add to database
            context.Farms.Add(addingFarm);
            await context.SaveChangesAsync(cancellationToken);
            // Return result
            return Result<Response>.Succeed(Response.FromEntity(addingFarm));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("api/farms", Handle)
            .WithTags("Farms")
            .WithMetadata(new SwaggerOperationAttribute("Create a Farm"))
            .RequireAuthorization();
        }
        public static async Task<IResult> Handle(ISender sender, string name, string owner, string address, string description, decimal rating, IFormFileCollection farmImages, CancellationToken cancellationToken = default)
        {
            Result<Response> result = await sender.Send(new Command(Name: name, Owner: owner, Address: address, Description: description, Rating: rating, FarmImages: farmImages), cancellationToken);
            if (!result.Succeeded)
                return Results.BadRequest(result);
            return Results.Created("", result);
        }
    }
}