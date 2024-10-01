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
        string? id =
            claims
                .FirstOrDefault(c =>
                    c.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty;
        string? username =
            claims
                .FirstOrDefault(c =>
                    c.Type.Equals("username", StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty;
        string? email =
            claims
                .FirstOrDefault(c =>
                    c.Type.Equals("email", StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty;
        string? phoneNumber =
            claims
                .FirstOrDefault(c =>
                    c.Type.Equals("phonenumber", StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty;
        string? imageUrl =
            claims
                .FirstOrDefault(c =>
                    c.Type.Equals("imageurl", StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty;
        User? checkingUser = await appContext.Users.SingleOrDefaultAsync(u => u.Id.Equals(id));
        if (checkingUser is null)
        {
            checkingUser = new()
            {
                Id = id,
                Username = username,
                PhoneNumber = phoneNumber,
                AvatarUrl = imageUrl,
                Email = email,
                Role = RoleEnum.Customer,
                Carts = new() { UserId = id }
            };
            appContext.Users.Add(checkingUser);
            await appContext.SaveChangesAsync(cancellationToken: default);
        }
        // then checking user is not null
        else
        {
            if (
                checkingUser.Username != username
                || checkingUser.Email != email
                || checkingUser.PhoneNumber != phoneNumber
                || checkingUser.AvatarUrl != imageUrl
            )
            {
                checkingUser.Username = username;
                checkingUser.Email = email;
                checkingUser.PhoneNumber = phoneNumber;
                checkingUser.AvatarUrl = imageUrl;
                await appContext.SaveChangesAsync(cancellationToken: default);
            }
        }
        CurrentUser currentUser = context.RequestServices.GetRequiredService<CurrentUser>();
        currentUser.User = checkingUser;
        await next.Invoke(context);
    }
}