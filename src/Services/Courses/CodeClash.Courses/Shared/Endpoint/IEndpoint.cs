namespace CodeClash.Courses.Shared.Endpoint;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder builder);
}
