using System.Reflection;

namespace Koi_Web_BE.Endpoints.Internal;

public static class EndpointExtensions
{
    public static void UseMinimalEndpoints<TMarker>(this IApplicationBuilder app)
    {
        UseMinimalEndpoints(app, typeof(TMarker));
    }

    private static void UseMinimalEndpoints(this IApplicationBuilder app, Type typeMarker)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!
                .Invoke(null, [app]);
        }
    }

    private static IEnumerable<TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
    {
        var endpointTypes = typeMarker.Assembly.DefinedTypes
            .Where(x => x is { IsAbstract: false, IsInterface: false } &&
                        typeof(IEndpoints).IsAssignableFrom(x));
        return endpointTypes;
    }
}
