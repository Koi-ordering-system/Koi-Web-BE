// using Koi_Web_BE.Database;
// using Koi_Web_BE.Endpoints.Internal;
// using Koi_Web_BE.Models;
// using Koi_Web_BE.Models.Entities;
// using Koi_Web_BE.Models.Primitives;
// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using Swashbuckle.AspNetCore.Annotations;

// namespace Koi_Web_BE.UseCases.UC_Species.Queries;

// public class GetSpecies
// {
//     public record Query(
//         string Keyword,
//         int PageIndex,
//         int PageSize
//     ) : IRequest<Result<PaginatedList<Response>>>;


//     public record Response(
//         Guid Id,
//         string Name,
//         string Description,
//         int YearOfDiscovery,
//         string DiscoveredBy,
//         DateTimeOffset CreatedAt
//     )
//     {
//         public static Response FromEntity(Species species)
//             => new(
//                 Id: species.Id,
//                 Name: species.Name,
//                 Description: species.Description,
//                 YearOfDiscovery: species.YearOfDiscovery,
//                 DiscoveredBy: species.DiscoveredBy,
//                 CreatedAt: species.CreatedAt
//             );
//     };

//     public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<PaginatedList<Response>>>
//     {
//         public async Task<Result<PaginatedList<Response>>> Handle(Query request, CancellationToken cancellationToken)
//         {
//             IQueryable<Species> query = context.Species
//                 .AsNoTracking()
//                 .Where(s => s.Name.Trim().ToLower().Contains(request.Keyword.Trim().ToLower()));
//             int total = await query.CountAsync(cancellationToken);
//             IEnumerable<Species> gettingSpecies = await query
//                 .Skip((request.PageIndex - 1) * request.PageSize)
//                 .Take(request.PageSize)
//                 .ToListAsync(cancellationToken);
//             return Result<PaginatedList<Response>>.Succeed(new(
//                 items: gettingSpecies
//                     .Select(Response.FromEntity).ToList(),
//                 count: total,
//                 pageNumber: request.PageIndex,
//                 pageSize: request.PageSize
//             ));
//         }
//     }

//     public class Endpoint : IEndpoints
//     {
//         public static void DefineEndpoints(IEndpointRouteBuilder app)
//         {
//             app.MapGet("/api/species", Handle)
//             .WithTags("Species")
//             .WithMetadata(new SwaggerOperationAttribute("Get all Species"))
//             .CacheOutput(b => b.Tag("Species"));
//         }
//         public static async Task<IResult> Handle(ISender sender, string keyword = "", int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
//         {
//             Result<PaginatedList<Response>> response = await sender.Send(new Query(keyword, pageIndex, pageSize), cancellationToken);
//             if (!response.Succeeded) return Results.NotFound(response);
//             return Results.Ok(response);
//         }
//     }
// }