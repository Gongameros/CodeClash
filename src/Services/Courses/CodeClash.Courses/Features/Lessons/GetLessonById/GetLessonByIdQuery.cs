using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Lessons.GetLessonById;

public sealed record GetLessonByIdQuery(string CourseId, string ModuleId, string LessonId)
    : IQuery<LessonDetailResponse>;