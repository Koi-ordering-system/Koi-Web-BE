using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace Koi_Web_BE.UseCases.UC_Users.Queries;

public class GetUserRole
{
    public class Endpoint : IEndpoints
    {
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users/role", Handle)
                .WithTags("Users")
                .WithMetadata(new SwaggerOperationAttribute("Get User Role"))
                .CacheOutput(b => b.Tag("Users"));
        }

        public static async Task<IResult> Handle(ISender sender, CurrentUser currentUser)
        {
            if (currentUser.User is null) return TypedResults.Unauthorized();
            await Task.Delay(0);
            return TypedResults.Ok(Result<string>.Succeed(currentUser.User.Role.ToString()));
        }
    }
}