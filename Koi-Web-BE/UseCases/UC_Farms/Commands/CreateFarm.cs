using CloudinaryDotNet.Actions;
using FluentValidation;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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

    public class Handler(IApplicationDbContext context, CurrentUser currentUser, IImageService imageService) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Check if the current user is an admin
            if (!currentUser.User!.IsAdmin())
                return Result<Response>.Fail(new ForbiddenException("The current user is not an admin"));
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
            .RequireAuthorization()
            .DisableAntiforgery();
        }
        public static async Task<IResult> Handle(
            ISender sender,
            [FromForm] string name,
            [FromForm] string owner,
            [FromForm] string address,
            [FromForm] string description,
            [FromForm] decimal rating,
            IFormFileCollection farmImages,
            CancellationToken cancellationToken = default)
        {
            Result<Response> result = await sender.Send(new Command(
                Name: name,
                Owner: owner,
                Address: address,
                Description: description,
                Rating: rating,
                FarmImages: farmImages), cancellationToken);
            if (!result.Succeeded)
                return Results.BadRequest(result);
            return Results.Created("", result);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Owner).NotEmpty().WithMessage("Owner is required.");
            RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.Rating).InclusiveBetween(0, 5).WithMessage("Rating must be between 0 and 5.");
            RuleFor(x => x.FarmImages)
                .Must(HaveValidImageSizes).WithMessage("All images must be less than 10MB.");
        }

        private bool HaveValidImageSizes(IFormFileCollection farmImages)
        {
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            return farmImages.All(image => image.Length <= maxFileSize);
        }
    }
}