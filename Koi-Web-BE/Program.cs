using Koi_Web_BE;
using Koi_Web_BE.Endpoints.Internal;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddMediatR(option =>
        {
            option.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

var app = builder.ConfigureServices();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMinimalEndpoints<Program>();

app.Run();
