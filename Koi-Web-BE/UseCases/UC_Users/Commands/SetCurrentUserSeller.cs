using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Users.Commands;

public class SetCurrentUserSeller
{
    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/users/seller", Handle)
                .WithMetadata(new SwaggerOperationAttribute("Update current user to seller"))
                .WithTags("Users")
                .RequireAuthorization();
        }
        public static async Task<IResult> Handle(ISender sender)
        {
            Result<Response> result = await sender.Send(new Command(), default);
            if (!result.Succeeded) return TypedResults.BadRequest();
            return TypedResults.NoContent();
        }
    }

    public record Command() : IRequest<Result<Response>>;

    public record Response;

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (currentUser.User is null) return Result<Response>.Fail(new Exception("You must have access token"));
            int result = await context.Users
                .Where(u => u.Id == currentUser.User.Id)
                .ExecuteUpdateAsync(e => e.SetProperty(u => u.Role, Models.Enums.RoleEnum.Seller), cancellationToken);
            if (result < 0) return Result<Response>.Fail(new Exception("User is not found"));
            return Result<Response>.Succeed(null!);
        }
    }
}