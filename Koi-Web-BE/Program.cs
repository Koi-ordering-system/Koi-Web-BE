using dotenv.net;
using Koi_Web_BE;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Middlewares;
using Koi_Web_BE.Models.Primitives;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// add MediatR
builder.Services.AddMediatR(option =>
{
    option.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// add ENV
DotEnv.Load();
var envVars = DotEnv.Read();
// add scoped
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<AuthMiddleware>();
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

// add database
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseNpgsql(envVars["CONNECTION_STRING"]);
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
    });
builder.Services.AddAuthorization();

var app = builder.ConfigureServices();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(option => option.DisplayRequestDuration());
    app.MigrateDatabase<ApplicationDbContext>(async (option, _) => await option.Seed());
}
app.MigrateDatabase<ApplicationDbContext>(async (_, _) => await Task.Delay(0));
app.UseMiddleware<AuthMiddleware>();

app.UseHttpsRedirection();

app.UseMinimalEndpoints<Program>();

app.Run();
