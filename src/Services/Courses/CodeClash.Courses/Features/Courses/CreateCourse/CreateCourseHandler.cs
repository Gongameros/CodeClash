using CodeClash.Courses.Domains.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Courses.CreateCourse;

public sealed class CreateCourseHandler(IMongoCollection<Course> courses)
    : ICommandHandler<CreateCourseCommand, CreateCourseResponse>
{
    public async ValueTask<Result<CreateCourseResponse>> Handle(
        CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var course = new Course
        {
            AuthorId = command.AuthorId,
            Title = command.Title,
            Description = command.Description,
            CodingTechnologies = command.CodingTechnologies,
            Difficulty = command.Difficulty,
            Tags = command.Tags,
            ThumbnailUrl = command.ThumbnailUrl
        };

        await courses.InsertOneAsync(course, cancellationToken: cancellationToken);

        return new CreateCourseResponse(course.Id);
    }
}