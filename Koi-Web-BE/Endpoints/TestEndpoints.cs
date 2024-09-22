using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.UseCases.Test.Queries;
using Koi_Web_BE.UseCases.UC_Species.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Koi_Web_BE.Endpoints;

public class TestEndpoints : IEndpoints
{
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/test", TestHandler);
        app.MapDelete("/users/{Id}", DeleteSpecies);
    }

    private static async Task<IResult> TestHandler([FromServices] ISender sender)
    {
        var command = new Test.Query();
        var result = await sender.Send(command);
        return Results.Ok(Result<Test.Response>.Succeed(result));
    }
    private static async Task<IResult> DeleteSpecies(ISender sender, Guid Id)
    {
        var result = await sender.Send(new DeleteSpecies.Command(Id));
        return Results.Ok(result);
    }
}