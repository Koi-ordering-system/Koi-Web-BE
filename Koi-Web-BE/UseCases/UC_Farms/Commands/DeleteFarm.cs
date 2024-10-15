using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Koi_Web_BE.UseCases.UC_Farms.Commands;

public class DeleteFarm
{
    public record Command(Guid Id) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context, CurrentUser currentUser, IImageService imageService, IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Check if the current user is an admin or manager
            if (!currentUser.User!.IsAdmin() && !currentUser.User!.IsManager())
                return Result<Response>.Fail(new ForbiddenException("The current user is not an admin or manager"));
            // Get the farm entity
            Farm? deletingFarm = await context.Farms
                .Include(farm => farm.FarmImages)
                .SingleOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (deletingFarm == null)
                return Result<Response>.Fail(new NotFoundException("Farm not found"));
            // Remove farm images in database
            context.FarmImages.RemoveRange(deletingFarm.FarmImages.ToList());
            // Remove farm images in cloudinary
            var deleteTasks = deletingFarm.FarmImages
                .Select(farmImage => imageService.DeleteImageAsync(farmImage.Url)).ToList();
            await Task.WhenAll(deleteTasks);
            if (deleteTasks.Any(task => !task.Result))
                return Result<Response>.Fail(new InvalidOperationException("Failed to delete images from Cloudinary"));
            // Remove the farm entity
            context.Farms.Remove(deletingFarm);
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
            app.MapDelete("api/farms/{id}", Handle)
            .WithTags("Farms")
            .WithMetadata(new SwaggerOperationAttribute("Delete a Farm"))
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