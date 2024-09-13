using Koi_Web_BE;
using Koi_Web_BE.Endpoints.Internal;

var builder = WebApplication.CreateBuilder(args);

var app = builder.ConfigureServices();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMinimalEndpoints<Program>();

app.Run();
