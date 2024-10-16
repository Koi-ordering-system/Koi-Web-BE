using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Extensions;
using Koi_Web_BE.Models;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;

namespace Koi_Web_BE.UseCases.UC_Kois.Queries;

public abstract class GetKois
{
    public class Query : IRequest<PaginatedList<ResponseItem>>
    {
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
        public string? Search { get; init; }
    }

    public class ResponseItem
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public bool IsMale { get; set; } = true;
        public decimal Price { get; set; } = 0;
        public ICollection<string> ImageUrls { get; set; } = new List<string>();
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PaginatedList<ResponseItem>>
    {
        public async Task<PaginatedList<ResponseItem>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.Kois
                .Include(x => x.FarmKois)
                .Include(x => x.Images)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(x => EF.Functions.ILike(x.Name, $"%{request.Search}%"));
            }
            Expression<Func<Koi, object>> keySelector = request.SortBy switch
            {
                "name" => x => x.Name,
                "description" => x => x.Description,
                _ => x => x.Name,
            };

            return await query.ListPaginateWithOrderAsync(
                request.Page,
                request.Size,
                keySelector,
                request.SortOrder,
                x => new ResponseItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    IsMale = x.IsMale,
                    MaxSize = x.MaxSize,
                    MinSize = x.MinSize,
                    ImageUrls = x.Images.Select(y => y.Url).ToList()
                });
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/kois", Handle)
                .WithTags("Kois")
                .WithMetadata(new SwaggerOperationAttribute("Get all kois"))
                .CacheOutput(b => b.Tag("Kois"));
        }

        public static async Task<IResult> Handle(ISender sender,
            [FromQuery] int? pageIndex,
            [FromQuery] int? pageSize,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] string? search,
            CancellationToken cancellationToken = default)
        {
            var response = await sender.Send(new Query
            {
                Page = pageIndex,
                Size = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Search = search,
            }, cancellationToken);
            return Results.Ok(Result<PaginatedList<ResponseItem>>.Succeed(response));
        }
    }
}
