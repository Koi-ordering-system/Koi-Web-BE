using Koi_Web_BE;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Middlewares;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// add MediatR
builder.Services.AddMediatR(option =>
{
    option.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<AuthMiddleware>();
builder.Services.AddOutputCache(builder =>
{
    builder.AddPolicy("default", policy =>
    {
        policy.Expire(TimeSpan.FromSeconds(10));
    });
});
builder.Services.AddSwaggerGen(option =>
{
    option.EnableAnnotations();
    option.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
        },
        Array.Empty<string>()
      }
    });
});


builder.Services.AddCors(option =>
{
    option.AddDefaultPolicy(option =>
    {
        option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://sharing-colt-41.clerk.accounts.dev";
        // options.Audience = audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/chat"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        }; options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/chat"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.ConfigureServices();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(option => option.DisplayRequestDuration());
    app.MigrateDatabase<ApplicationDbContext>(async (option, _) => await option.Seed());
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(option => option.DisplayRequestDuration());
    app.MigrateDatabase<ApplicationDbContext>(async (_, _) => await Task.Delay(0));
}
app.UseCors();
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromHours(1)
};

webSocketOptions.AllowedOrigins.Add("*");
app.UseWebSockets(webSocketOptions);
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.MapHub<ChatHub>("/chat");

app.UseHttpsRedirection();

app.UseMinimalEndpoints<Program>();
app.Run();
