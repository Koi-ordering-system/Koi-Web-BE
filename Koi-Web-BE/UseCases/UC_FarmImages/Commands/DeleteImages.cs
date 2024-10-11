using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sprache;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_FarmImages.Commands;

public class DeleteImages
{
    public record Command(Guid FarmId, IEnumerable<Guid> ImageIds) : IRequest<Result<Response>>;

    public record Response();

    public class Handler(IApplicationDbContext context, CurrentUser currentUser, IImageService imageService) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            //check whether user has permission
            if (!currentUser.User!.IsAdmin())
                return Result<Response>.Fail(new ForbiddenException("The current user is not an admin"));
            //check request images are existed
            var deleteImages = context.FarmImages
                .AsNoTracking()
                .Where(fi => request.ImageIds.Contains(fi.Id) && fi.FarmId == request.FarmId);
            if (deleteImages.Count() != request.ImageIds.Count())
                return Result<Response>.Fail(new NotFoundException("request contain file id does not exist"));

            //delete files from database
            context.FarmImages.RemoveRange(deleteImages);
            // Remove farm images in cloudinary
            var deleteTasks = deleteImages
                .Select(farmImage => imageService.DeleteImageAsync(farmImage.Url)).ToList();
            await Task.WhenAll(deleteTasks);
            if (deleteTasks.Any(task => !task.Result))
                return Result<Response>.Fail(new InvalidOperationException("Failed to delete images from Cloudinary"));
            //Save changes to the database
            await context.SaveChangesAsync(cancellationToken);
            return Result<Response>.Succeed(null!);
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapDelete("", Handle)
            .WithTags("FarmImages")
            .WithMetadata(new SwaggerOperationAttribute("Delete Farm Images"))
            .RequireAuthorization();
        }

        public static async Task<IResult> Handle(ISender sender,
            Guid farmId,
            Guid[] imageIds,
            CancellationToken cancellationToken = default)
        {
            Result<Response> result = await sender.Send(
                new Command(farmId, imageIds),
                cancellationToken
            );
            if (!result.Succeeded)
                return Results.BadRequest();
            return Results.NoContent();
        }
    }
}