using FluentValidation;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Feedbacks.Commands
{
    public class UpdateFeedback
    {
        public record Command(Guid Id, int Rating, string Content) : IRequest<Result<Response>>;

        public class UpdateFeedbackRequest
        {
            public int Rating { get; set; } = 5;
            public string Content { get; set; } = string.Empty;
        };

        public record Response(
            Guid Id,
            Guid OrderId,
            string UserId,
            int Rating,
            string Content)
        {
            public static Response FromEntity(Review review)
                => new(review.Id, review.OrderId, review.UserId, review.Rating, review.Content);
        };

        public class Handler(
            IApplicationDbContext context,
            CurrentUser currentUser,
            IOutputCacheStore store
            ) : IRequestHandler<Command, Result<Response>>
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

                review.Rating = request.Rating;
                review.Content = request.Content;

                context.Reviews.Update(review);
                await context.SaveChangesAsync(cancellationToken);
                await store.EvictByTagAsync("Feedbacks", cancellationToken);

                var newReview = await context.Reviews.Where(r => r.Id == request.Id).SingleOrDefaultAsync(cancellationToken);
                if (newReview is null)
                    return Result<Response>.Fail(new NotFoundException("Feedback not found"));
                return Result<Response>.Succeed(Response.FromEntity(newReview));
            }
        }

        public class Endpoint : IEndpoints
        {
            public static void DefineEndpoints(IEndpointRouteBuilder app)
            {
                app.MapPut("/api/feedbacks/{id}", Handle)
                    .WithTags("Feedbacks")
                    .WithMetadata(new SwaggerOperationAttribute("Update a Feedback"))
                    .RequireAuthorization();
            }

            public static async Task<IResult> Handle(
                ISender sender,
                Guid id,
                [FromBody] UpdateFeedbackRequest request,
                CancellationToken cancellationToken = default)
            {
                Command command = new(id, request.Rating, request.Content);
                var result = await sender.Send(command, cancellationToken);
                if (!result.Succeeded)
                    return Results.BadRequest(result);
                return Results.Ok(result);
            }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Rating).NotEmpty()
                        .GreaterThanOrEqualTo(1)
                        .LessThanOrEqualTo(5);
                    RuleFor(x => x.Content).NotEmpty();
                }
            }
        }
    }
}