using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Courses.GetCourseById;

public sealed record GetCourseByIdQuery(string CourseId) : IQuery<CourseDetailResponse>;