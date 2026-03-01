using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Courses.DeleteCourse;

public sealed record DeleteCourseCommand(string CourseId, string AuthorId) : ICommand;
