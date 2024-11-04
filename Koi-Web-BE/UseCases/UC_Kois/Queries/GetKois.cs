using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Extensions;
using Koi_Web_BE.Models;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;

namespace Koi_Web_BE.UseCases.UC_Kois.Queries;

public abstract class GetKois
{
    public record Query(
        int Page,
        int Size,
        string Search,
        Guid? Id
    ) : IRequest<PaginatedList<ResponseItem>>;

    public record GetKoisRequest(
        int pageIndex = 1,
        int pageSize = 10,
        string keyword = "",
        Guid? id = null!
    );

    public class ResponseItem
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public string[] ImageUrls { get; set; } = [];
        public string[] Colors { get; set; } = [];
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PaginatedList<ResponseItem>>
    {
        public async Task<PaginatedList<ResponseItem>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.Kois
                .Include(x => x.FarmKois)
                .Include(x => x.Images)
                .Include(x => x.Colors)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(x => EF.Functions.ILike(x.Name, $"%{request.Search}%"));
            }

            if (request.Id is not null)
            {
                query = query.Where(x => x.FarmKois.Any(f => f.FarmId == request.Id));
            }

            Expression<Func<Koi, object>> keySelector = x => x.Name;

            return await query.ListPaginateWithOrderAsync(
                request.Page,
                request.Size,
                keySelector,
                "Descending",
                x => new ResponseItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    MaxSize = x.MaxSize,
                    MinSize = x.MinSize,
                    ImageUrls = x.Images.Select(y => y.Url).ToArray(),
                    Colors = x.Colors.Select(y => y.Name).ToArray(),
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
            [AsParameters] GetKoisRequest request)
        {
            var response = await sender.Send(new Query(
                request.pageIndex,
                request.pageSize,
                request.keyword,
                request.id
            ), default);
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
