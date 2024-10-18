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
        public QueryFilter? Filter { get; init; }
    }

    public class QueryFilter
    {
        public string? Search { get; init; }
        public int? FromPrice { get; init; }
        public int? ToPrice { get; init; }
        public Guid[]? TypeIds { get; init; }
        public string[]? Colors { get; init; }
        public (decimal MinSize, decimal MaxSize)[]? Sizes { get; init; }
    }

    public class ResponseItem
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public ICollection<string> ImageUrls { get; set; } = [];
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PaginatedList<ResponseItem>>
    {
        public async Task<PaginatedList<ResponseItem>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.Kois
                .Include(x => x.FarmKois)
                .Include(x => x.Images)
                .AsNoTracking();

            if (request.Filter is not null)
            {
                if (!string.IsNullOrEmpty(request.Filter.Search))
                {
                    query = query.Where(x => EF.Functions.ILike(x.Name, $"%{request.Filter.Search}%"));
                }

                if (request.Filter.FromPrice is not null)
                {
                    query = query.Where(x => x.Price >= request.Filter.FromPrice);
                }

                if (request.Filter.ToPrice is not null)
                {
                    query = query.Where(x => x.Price <= request.Filter.ToPrice);
                }

                if (request.Filter.Colors is not null)
                {
                    query = query.Where(x => x.Colors.Any(y => request.Filter.Colors.Contains(y.Name)));
                }

                if (request.Filter.Sizes is not null)
                {
                    query = query.Where(x => request.Filter.Sizes.Any(s => s.MinSize <= x.MinSize && x.MaxSize <= s.MaxSize));
                }
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
                    MaxSize = x.MaxSize,
                    MinSize = x.MinSize,
                    ImageUrls = x.Images.Select(y => y.Url).ToList(),
                });
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/kois/get", Handle)
                .WithTags("Kois")
                .WithMetadata(new SwaggerOperationAttribute("Get all kois"))
                .CacheOutput(b => b.Tag("Kois"));
        }

        public static async Task<IResult> Handle(ISender sender,
            [FromBody] GetKoiRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = await sender.Send(new Query
            {
                Page = request.PageIndex,
                Size = request.PageSize,
                SortBy = request.SortBy,
                SortOrder = request.SortOrder,
                Filter = new QueryFilter
                {
                    Search = request.Search,
                    FromPrice = request.FromPrice,
                    ToPrice = request.ToPrice,
                    TypeIds = request.TypeIds,
                    Colors = request.Colors,
                    Sizes = request.Sizes?.Select(x => (x.MinSize, x.MaxSize)).ToArray(),
                }
            }, cancellationToken);
            return Results.Ok(Result<PaginatedList<ResponseItem>>.Succeed(response));
        }

        public class GetKoiRequest
        {
            public int? PageIndex { get; init; }
            public int? PageSize { get; init; }
            public string? SortBy { get; init; }
            public string? SortOrder { get; init; }
            public string? Search { get; init; }
            public int? FromPrice { get; init; }
            public int? ToPrice { get; init; }
            public Guid[]? TypeIds { get; init; }
            public string[]? Colors { get; init; }
            public RequestSize[]? Sizes { get; init; }
        }

        public class RequestSize
        {
            public decimal MinSize { get; init; }
            public decimal MaxSize { get; init; }
        }
    }
}
