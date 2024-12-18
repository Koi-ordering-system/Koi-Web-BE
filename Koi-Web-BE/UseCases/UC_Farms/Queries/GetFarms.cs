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

namespace Koi_Web_BE.UseCases.UC_Farms.Queries;

public class GetFarms
{
    public record Query(
        int Page,
        int Size,
        string SortBy,
        string SortOrder,
        string Name
    ) : IRequest<PaginatedList<FarmResponse>>;

    public record KoiResponse(
        Guid Id,
        string Name,
        int Quantity,
        IEnumerable<string> ImageUrls,
        IEnumerable<ColorResponse> Colors
    );

    public record ColorResponse(
        Guid Id,
        string Name
    );

    public record ImageResponse(
        Guid Id,
        string Url
    )
    {
        public static ImageResponse FromEntity(FarmImage farmImage)
            => new(
                Id: farmImage.Id,
                Url: farmImage.Url
            );
    };

    public record FarmResponse(
        Guid Id,
        string Name,
        string Owner,
        string Address,
        string Description,
        decimal Rating,
        IEnumerable<KoiResponse> Kois,
        IEnumerable<ImageResponse> FarmImages
    )
    {

        public static FarmResponse FromEntity(Farm farm)
            => new(
                Id: farm.Id,
                Name: farm.Name,
                Owner: farm.Owner,
                Address: farm.Address,
                Description: farm.Description,
                Rating: farm.Rating,
                Kois: farm.FarmKois.Select(farmKoi => new KoiResponse(farmKoi.Koi.Id,
                                                                                farmKoi.Koi.Name,
                                                                                farmKoi.Quantity,
                                                                                farmKoi.Koi.Images.Select(image => image.Url),
                                                                                farmKoi.Koi.Colors.Select(c => new ColorResponse(
                                                                                    c.Id, c.Name
                                                                                )).ToList())),
                FarmImages: farm.FarmImages.Select(ImageResponse.FromEntity)
            );
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PaginatedList<FarmResponse>>
    {
        public async Task<PaginatedList<FarmResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Farm> query = context.Farms
                .AsNoTracking()
                .AsSplitQuery()
                .Include(f => f.FarmImages)
                .Include(f => f.FarmKois).ThenInclude(f => f.Koi).ThenInclude(f => f.Images)
                .Include(f => f.FarmKois).ThenInclude(f => f.Koi).ThenInclude(f => f.Colors)
                .Where(f => f.Name.Trim().ToLower().Contains(request.Name.Trim().ToLower()));

            //sort
            Expression<Func<Farm, object>> keySelector = request.SortBy switch
            {
                "name" => x => x.Name,
                "description" => x => x.Description,
                "Owner" => x => x.Owner,
                "address" => x => x.Address,
                "rating" => x => x.Rating,
                _ => x => x.Name,
            };


            return await query.ListPaginateWithOrderAsync(
                request.Page,
                request.Size,
                keySelector,
                request.SortOrder,
                FarmResponse.FromEntity
            );
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/farms", Handle)
                .WithTags("Farms")
                .WithMetadata(new SwaggerOperationAttribute("Get all Farms"))
                .CacheOutput(b => b.Tag("Farms"));
        }

        public static async Task<IResult> Handle(ISender sender,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "",
            [FromQuery] string sortOrder = "",
            [FromQuery] string search = "",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sortOrder) || sortOrder.Length < 3)
                sortOrder = "Ascending";
            var response = await sender.Send(new Query(
                pageIndex,
                pageSize,
                sortBy.ToLower(),
                char.ToUpper(sortOrder[0]) + sortOrder[1..].ToLower(),
                search
            ), cancellationToken);
            return Results.Ok(Result<PaginatedList<FarmResponse>>.Succeed(response));
        }
    }
}