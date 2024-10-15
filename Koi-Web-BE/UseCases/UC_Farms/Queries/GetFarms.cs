using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Farms.Queries;

public class GetFarms
{
    public record Query(
        int PageIndex,
        int PageSize,
        bool AscByRating,
        string Name
    ) : IRequest<Result<Response>>;

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
                FarmImages: farm.FarmImages.Select(i => ImageResponse.FromEntity(i))
            );
    }

    public record Response(
        int PageIndex,
        int PageSize,
        int TotalPages,
        IEnumerable<FarmResponse> Farms
    );

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Farm> query = context.Farms
                .AsNoTracking()
                .Include(f => f.FarmImages)
                .Where(f => f.Name.ToLower().Contains(request.Name.ToLower()));
            //calculate total pages
            var totalFarms = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalFarms / (double)request.PageSize);

            //sort
            if (request.AscByRating)
                query = query.OrderBy(f => f.Rating);
            else
                query = query.OrderByDescending(f => f.Rating);
            //map to response detail
            IEnumerable<FarmResponse> farms = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => FarmResponse.FromEntity(f))
                .ToListAsync(cancellationToken);
            //return response
            return Result<Response>.Succeed(new Response(
                PageIndex: request.PageIndex,
                PageSize: request.PageSize,
                TotalPages: totalPages,
                Farms: farms
            ));
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
            int pageIndex = 1,
            int pageSize = 10,
            bool ascByRating = false,
            string search = "",
            CancellationToken cancellationToken = default)
        {
            Result<Response> response = await sender.Send(new Query(pageIndex, pageSize, ascByRating, search), cancellationToken);
            return Results.Ok(response);
        }
    }
}