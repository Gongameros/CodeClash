using CodeClash.Courses.Shared.Endpoint;
using CodeClash.Results;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CodeClash.Courses.Features.Courses.GetCoderCourses;

public sealed record CourseResponse(
    Guid Id,
    string Title,
    string Description,
    decimal Price);

public sealed record GetCoursesQuery(int Page = 1, int PageSize = 10)
    : Shared.Messaging.IQuery<IReadOnlyList<CourseResponse>>;

public sealed class GetCoursesQueryValidator : AbstractValidator<GetCoursesQuery>
{
    public GetCoursesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}

public sealed class GetCoursesHandler : Shared.Messaging.IQueryHandler<GetCoursesQuery, IReadOnlyList<CourseResponse>>
{
    private static readonly List<CourseResponse> Courses =
    [
        new(Guid.Parse("a1b2c3d4-0001-0000-0000-000000000000"), "C# Fundamentals", "Learn C# from scratch", 49.99m),
        new(Guid.Parse("a1b2c3d4-0002-0000-0000-000000000000"), "ASP.NET Core Minimal APIs", "Build modern APIs", 79.99m),
        new(Guid.Parse("a1b2c3d4-0003-0000-0000-000000000000"), "Domain-Driven Design", "Strategic and tactical DDD patterns", 99.99m)
    ];

    public async ValueTask<Result<IReadOnlyList<CourseResponse>>> Handle(
        GetCoursesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<CourseResponse> courses = Courses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList()
            .AsReadOnly();

        return Result.Success(courses);
    }
}

public class GetCoursesEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/courses", Handle)
            .WithName("GetCourses")
            .WithTags("Courses");
    }

    private static async Task<IResult> Handle(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetCoursesQuery(page, pageSize), cancellationToken)).ToProblemDetails();
    }
}
