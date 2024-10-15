using FluentValidation;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Farms.Commands;

public class UpdateFarm
{
    public record Command(
        Guid Id,
        string Name,
        string Owner,
        string Address,
        string Description,
        IFormFileCollection FarmImages
    ) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context, CurrentUser currentUser, IImageService imageService, IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Check if the current user is an admin or manager
            if (!currentUser.User!.IsAdmin() && !currentUser.User!.IsManager())
                return Result<Response>.Fail(new ForbiddenException("The current user is not an admin or manager"));
            // Get the farm entity
            Farm? updatingFarm = await context.Farms
                .Include(farm => farm.FarmImages)
                .SingleOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (updatingFarm == null)
                return Result<Response>.Fail(new NotFoundException("Farm not found"));
            // Update the farm entity
            updatingFarm.Update(request.Name, request.Owner, request.Address, request.Description);
            // Upload images to Cloudinary and add URLs to the farm
            var uploadTasks = request.FarmImages.Select(async image =>
            {
                var imageUrl = await imageService.UploadImageAsync(image, image.FileName, "farms");
                return new FarmImage
                {
                    FarmId = updatingFarm.Id,
                    Url = imageUrl
                };
            });
            context.FarmImages.AddRange(await Task.WhenAll(uploadTasks));
            // Save changes to the database
            await context.SaveChangesAsync(cancellationToken);
            // clear cache
            await store.EvictByTagAsync("Farms", cancellationToken);
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPut("api/farms/{id}", Handle)
                .WithTags("Farms")
                .WithMetadata(new SwaggerOperationAttribute("Update a Farm"))
                .RequireAuthorization()
                .DisableAntiforgery();
        }

        public static async Task<IResult> Handle(
            ISender sender,
            [FromRoute] Guid id,
            [FromForm] string name,
            [FromForm] string owner,
            [FromForm] string address,
            [FromForm] string description,
            IFormFileCollection farmImages,
            CancellationToken cancellationToken = default)
        {
            Result<Response> result = await sender.Send(new Command(id, name, owner, address, description, farmImages), cancellationToken);
            if (!result.Succeeded)
                return Results.BadRequest(result);
            return Results.NoContent();
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