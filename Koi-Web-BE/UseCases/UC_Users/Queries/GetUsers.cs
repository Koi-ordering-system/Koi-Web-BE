using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Users.Queries;

public class GetUsers
{
    public record GetUsersRequest(
        int PageIndex = 1,
        int PageSize = 10
    );

    public record Query(
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<Result<PaginatedList<Response>>>;

    public record Response(
        string Id,
        string Username,
        string AvatarUrl,
        string Email,
        string PhoneNumber,
        string Role
    )
    {
        public static Response FromEntity(User user) => new(
            Id: user.Id,
            Username: user.Username,
            AvatarUrl: user.AvatarUrl,
            Email: user.Email,
            PhoneNumber: user.PhoneNumber,
            Role: user.Role.ToString() ?? string.Empty
        );
    };

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<PaginatedList<Response>>>
    {
        public async Task<Result<PaginatedList<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<User> query = context.Users
                .AsNoTracking();
            int total = await query.CountAsync(cancellationToken);
            IEnumerable<Response> gettingUsers = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => Response.FromEntity(u))
                .ToListAsync(cancellationToken);
            return Result<PaginatedList<Response>>.Succeed(new(
                items: gettingUsers.ToList(),
                count: total,
                pageNumber: request.PageIndex,
                pageSize: request.PageSize
            ));
        }
    }

    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users", async (ISender sender, int pageIndex = 1, int pageSize = 10) =>
            {
                Result<PaginatedList<Response>> response = await sender.Send(new Query(pageIndex, pageSize), default);
                return TypedResults.Ok(response);   
            })
                .WithTags("Users")
                .WithMetadata(new SwaggerOperationAttribute("Get all Users"))
                .CacheOutput(b => b.Tag("Users"));
        }
    }
}