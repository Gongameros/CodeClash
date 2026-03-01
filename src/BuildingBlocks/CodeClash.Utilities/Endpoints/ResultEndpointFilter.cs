using CodeClash.Results;
using Microsoft.AspNetCore.Http;

namespace CodeClash.Utilities.Endpoints;

public class ResultEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var result = await next(context);

        // Adjust this to match your Result types
        if (result is Result resultBase)
        {
            return resultBase.ToProblemDetails();
        }

        return result;
    }
}
