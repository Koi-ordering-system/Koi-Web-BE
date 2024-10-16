using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Feedbacks.Commands
{
    public class RemoveFeedback
    {
        public record Command(Guid Id) : IRequest<Result<Response>>;
        public record Response();
        
        public class Handler(IApplicationDbContext context, CurrentUser currentUser, IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
        {
            public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
            {
                
                Review? review = await context.Reviews.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                
                if (review is null)
                    return Result<Response>.Fail(new NotFoundException("Feedback not found"));
                bool isFeedbackUser = review.UserId == currentUser.User!.Id;
                if (!isFeedbackUser)
                    return Result<Response>.Fail(new ForbiddenException("You are not allowed to delete this feedback"));
                context.Reviews.Remove(review);
                await context.SaveChangesAsync(cancellationToken);
                await store.EvictByTagAsync("Feedbacks", cancellationToken);
                return Result<Response>.Succeed(null);
            }
        }

        public class Endpoint : IEndpoints
        {
            public static void DefineEndpoints(IEndpointRouteBuilder app)
            {
                app.MapDelete("/api/feedbacks/{id}", Handle)
                    .WithTags("Feedbacks")
                    .WithMetadata(new SwaggerOperationAttribute("Remove a Feedback"))
                    .RequireAuthorization();
            }

            public static async Task<IResult> Handle(ISender sender, Guid id,
                CancellationToken cancellationToken = default)
            {
                var result = await sender.Send(new Command(id), cancellationToken);
                if (!result.Succeeded)
                    return Results.BadRequest(result);
                return Results.NoContent();
            }
        }
    }
}