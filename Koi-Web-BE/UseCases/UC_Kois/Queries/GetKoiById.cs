using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Kois.Queries;

public class GetKoiById
{
    public record Query(Guid Id) : IRequest<Response>;

    public class Response
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal MinSize { get; set; } = 0;
        public decimal MaxSize { get; set; } = 0;
        public bool IsMale { get; set; } = true;
        public decimal Price { get; set; } = 0;
        public ICollection<ResponseFarm> Farms { get; set; } = new List<ResponseFarm>();
        public ICollection<string> ImageUrls { get; set; } = new List<string>();
    }

    public class ResponseFarm
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            Koi? koi = await context.Kois
                .Include(x => x.FarmKois)
                .ThenInclude(y => y.Farm)
                .Include(x => x.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (koi is null)
            {
                throw new NotFoundException("Koi not found");
            }

            return new Response
            {
                Name = koi.Name,
                Description = koi.Description,
                Price = koi.Price,
                IsMale = koi.IsMale,
                MaxSize = koi.MaxSize,
                MinSize = koi.MinSize,
                Farms = koi.FarmKois.Select(fk => new ResponseFarm
                {
                    Id = fk.Farm.Id,
                    Name = fk.Farm.Name,
                }).ToList(),
                ImageUrls = koi.Images.Select(x => x.Url).ToList()
            };
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/kois/{id:guid}", Handle)
                .WithTags("Kois")
                .WithMetadata(new SwaggerOperationAttribute("Get a Koi"))
                .CacheOutput(b => b.Tag("Kois"));
        }

        private static async Task<IResult> Handle(ISender sender, [FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            Response response = await sender.Send(new Query(id), cancellationToken);
            return Results.Ok(Result<Response>.Succeed(response));
        }
    }
}