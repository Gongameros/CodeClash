using Microsoft.AspNetCore.Routing;

namespace CodeClash.Utilities.Endpoints;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder builder);
}
