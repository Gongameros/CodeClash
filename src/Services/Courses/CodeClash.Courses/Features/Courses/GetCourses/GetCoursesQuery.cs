using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Courses.GetCourses;

public sealed record GetCoursesQuery(int Page = 1, int PageSize = 10)
    : IQuery<IReadOnlyList<CourseListItem>>;