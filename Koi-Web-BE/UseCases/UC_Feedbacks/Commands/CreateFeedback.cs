using FluentValidation;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Feedbacks.Commands
{
    public class CreateFeedback
    {
        public record Command(Guid OrderId, int Rating, string Content) : IRequest<Result<Response>>;

        public class CreateFeedbackRequest
        {
            public Guid OrderId { get; set; }
            public int Rating { get; set; } = 0;
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

        public class Handler(IApplicationDbContext context, CurrentUser currentUser, IOutputCacheStore store) : IRequestHandler<Command, Result<Response>>
        {
            public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
            {
                var existedReview = await context.Reviews
                    .AsNoTracking()
                    .AnyAsync(r => r.OrderId == request.OrderId && r.UserId.Equals(currentUser.User!.Id), cancellationToken);
                if (existedReview)
                    return Result<Response>.Fail(new InvalidOperationException("Already exists!"));
                Review review = new()
                {
                    UserId = currentUser.User!.Id,
                    OrderId = request.OrderId,
                    Rating = request.Rating,
                    Content = request.Content
                };
                context.Reviews.Add(review);
                await context.SaveChangesAsync(cancellationToken);
                await store.EvictByTagAsync("Feedbacks", cancellationToken);
                var newReview = await context.Reviews.Where(r => r.UserId == review.UserId && r.OrderId == review.OrderId).SingleOrDefaultAsync(cancellationToken);
                return Result<Response>.Succeed(Response.FromEntity(newReview!));
            }
        }

        public class Endpoint : IEndpoints
        {
            public static void DefineEndpoints(IEndpointRouteBuilder app)
            {
                app.MapPost("api/feedbacks", Handle)
                    .WithTags("Feedbacks")
                    .WithMetadata(new SwaggerOperationAttribute("Create a Feedback for farm"))
                    .RequireAuthorization();
            }

            public static async Task<IResult> Handle(
                ISender sender,
                [FromBody] CreateFeedbackRequest request,
                CancellationToken cancellationToken = default)
            {
                var result = await sender.Send(new Command(
                    request.OrderId,
                    request.Rating,
                    request.Content
                    ), cancellationToken);

                if (!result.Succeeded)
                    return Results.BadRequest(result);
                return Results.Created("feedback", result);
            }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.OrderId).NotEmpty();
                RuleFor(x => x.Rating).NotEmpty()
                    .GreaterThanOrEqualTo(1)
                    .LessThanOrEqualTo(5);
                RuleFor(x => x.Content).NotEmpty();
            }
        }
    }
}