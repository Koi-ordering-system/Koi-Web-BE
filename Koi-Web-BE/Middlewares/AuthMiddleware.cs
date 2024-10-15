using Koi_Web_BE.Database;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Koi_Web_BE.Middlewares;

public class AuthMiddleware(IApplicationDbContext appContext) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string issuer = "https://sharing-colt-41.clerk.accounts.dev";
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.IsNullOrEmpty() || !authHeader.Contains("Bearer "))
        {
            await next.Invoke(context);
            return;
        }
        authHeader = authHeader.Replace("Bearer ", "");
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(authHeader);
        var claims = jwtSecurityToken.Claims;
        if (!claims.Any(c => c.Issuer.Equals(issuer, StringComparison.InvariantCultureIgnoreCase)))
        {
            await next.Invoke(context);
            return;
        }
        var idClaim = claims.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.InvariantCultureIgnoreCase));
        string? id = idClaim?.Value ?? string.Empty;
        User? checkingUser = await appContext.Users.SingleOrDefaultAsync(u => u.Id.Equals(id));
        if (checkingUser is null)
        {
            checkingUser = new()
            {
                Id = id,
                Username = "ADMIN",
                PhoneNumber = "0938386853",
                AvatarUrl = "https://img.icons8.com/color/48/000000/administrator-male.png",
                Email = "CjT5A@example.com",
                Role = RoleEnum.Admin,
                Carts = new() { UserId = id }
            };
            appContext.Users.Add(checkingUser);
            await appContext.SaveChangesAsync(cancellationToken: default);
        }
        // then checking user is not null
        CurrentUser currentUser = context.RequestServices.GetRequiredService<CurrentUser>();
        currentUser.User = checkingUser;
        await next.Invoke(context);
    }
}