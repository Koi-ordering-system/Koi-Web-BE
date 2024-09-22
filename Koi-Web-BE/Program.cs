using Koi_Web_BE;
using Koi_Web_BE.Database;
using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// add MediatR
builder.Services.AddMediatR(option =>
    {
        option.RegisterServicesFromAssembly(typeof(Program).Assembly);
    });

// add scoped
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

// add database
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.ConfigureServices();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MigrateDatabase<ApplicationDbContext>(async (option, _) => await option.Seed());
}
app.MigrateDatabase<ApplicationDbContext>(async (_, _) => await Task.Delay(0));

app.UseHttpsRedirection();

app.UseMinimalEndpoints<Program>();

app.Run();
