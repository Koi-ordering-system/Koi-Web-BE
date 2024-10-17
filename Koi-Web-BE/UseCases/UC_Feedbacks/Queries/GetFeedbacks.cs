using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Extensions;
using Koi_Web_BE.Models;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.UseCases.UC_Farms.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;

namespace Koi_Web_BE.UseCases.UC_Feedbacks.Queries
{
    public class GetFeedbacks
    {
        public record Query(
            int Page,
            int Size,
            string SortBy,
            string SortOrder,
            Guid FarmId
        ) : IRequest<PaginatedList<Response>>;

        public record Response(
            Guid Id,
            Guid FarmId,
            string UserId,
            int Rating,
            string Content)
        {
            public static Response FromEntity(Review review)
                => new(review.Id, review.FarmId, review.UserId, review.Rating, review.Content);
        }
        
        public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PaginatedList<Response>>
        {
            public async Task<PaginatedList<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = context.Reviews
                    .AsNoTracking()
                    .Where(r => r.FarmId == request.FarmId); 
                
                // bool isSorting = !string.IsNullOrEmpty(request.SortBy); 
                // sort
                Expression<Func<Review, object>> keySelector = request.SortBy switch
                {
                    "rating" => r => r.Rating,
                    _ => r => r.Rating,
                };

                // if (isSorting)
                return await query.ListPaginateWithOrderAsync(
                    request.Page,
                    request.Size,
                    keySelector,
                    request.SortOrder,
                    Response.FromEntity
                );
            }
        }

        public class Endpoint : IEndpoints
        {
            public static void DefineEndpoints(IEndpointRouteBuilder app)
            {
                app.MapGet("/api/farms/{farmId}/feedbacks", Handle)
                    .WithTags("Farms")
                    .WithMetadata(new SwaggerOperationAttribute("Get feedbacks of a Farm"))
                    .CacheOutput(b => b.Tag("Feedbacks"));
            }

            public static async Task<IResult> Handle(ISender sender,
                Guid farmId,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string sortBy = "",
                [FromQuery] string sortOrder = "",
                CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrEmpty(sortOrder) || sortOrder.Length < 3)
                    sortOrder = "Ascending";
                var response = await sender.Send(new Query(
                    pageIndex,
                    pageSize,
                    sortBy.ToLower(),
                    char.ToUpper(sortOrder[0]) + sortOrder[1..].ToLower(),
                    farmId
                    ), cancellationToken);
                return Results.Ok(Result<PaginatedList<Response>>.Succeed(response));
            }
        }
    }
}