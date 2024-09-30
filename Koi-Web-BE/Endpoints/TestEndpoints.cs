using Koi_Web_BE.Endpoints.Internal;
using Koi_Web_BE.Models.Primitives;
using Koi_Web_BE.UseCases.Test.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Koi_Web_BE.Endpoints;

public class TestEndpoints : IEndpoints
{
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        // GET
        app.MapGet("/api/test", TestHandler);
        app.MapGroup("api/species").DefineSpeciesEndpoints().WithTags("Species");
    }

    private static async Task<IResult> TestHandler([FromServices] ISender sender)
    {
        var command = new Test.Query();
        var result = await sender.Send(command);
        return Results.Ok(Result<Test.Response>.Succeed(result));
    }
}