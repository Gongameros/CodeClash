using CodeClash.Identity.Extensions;
using CodeClash.Web.ApiClients.Models.Enums;
using CodeClash.Web.ApiClients.Models.Requests;
using CodeClash.Web.ApiClients.Models.Responses;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace CodeClash.Web.Components.Pages.Courses;

public partial class EditLesson
{
    [Parameter] public string CourseId { get; set; } = string.Empty;
    [Parameter] public string ModuleId { get; set; } = string.Empty;
    [Parameter] public string LessonId { get; set; } = string.Empty;
    [CascadingParameter] private Task<AuthenticationState> AuthState { get; set; } = default!;

    private LessonDetailResponse? _lesson;
    private bool _loading = true;
    private bool _saving;

    private readonly LessonEditModel _form = new();

    private List<BreadcrumbItem> _breadcrumbs =
    [
        new("Home", href: "/"),
        new("Courses", href: "/courses"),
        new("Course", href: null, disabled: true),
        new("Edit Lesson", href: null, disabled: true)
    ];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var course = await CoursesApi.GetCourseByIdAsync(CourseId);
            if (course is not null)
            {
                var authState = await AuthState;
                var userId = authState.User.GetUserId();
                if (userId is null || userId != course.AuthorId)
                {
                    Navigation.NavigateTo($"/courses/{CourseId}", replace: true);
                    return;
                }
            }

            _lesson = await CoursesApi.GetLessonByIdAsync(CourseId, ModuleId, LessonId);
            if (_lesson is not null)
            {
                _form.Title = _lesson.Title;
                _form.Type = _lesson.Type;
                _form.Order = _lesson.Order;
                _form.Content = _lesson.Content ?? string.Empty;

                _breadcrumbs[2] = new("Course", href: $"/courses/{CourseId}");
                _breadcrumbs[3] = new(_lesson.Title, href: null, disabled: true);
            }
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(_form.Title)) return;
        _saving = true;
        try
        {
            await CoursesApi.UpdateLessonAsync(CourseId, ModuleId, LessonId,
                new UpdateLessonRequest(
                    _form.Title,
                    _form.Type,
                    _form.Order,
                    _form.Type == LessonType.Theory ? _form.Content : null,
                    null));

            Snackbar.Add("Lesson saved!", Severity.Success);
            Navigation.NavigateTo($"/courses/{CourseId}");
        }
        catch
        {
            Snackbar.Add("Failed to save lesson", Severity.Error);
        }
        finally { _saving = false; }
    }

    private static string RenderMarkdown(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
        return Markdown.ToHtml(markdown, pipeline);
    }

    private class LessonEditModel
    {
        public string Title { get; set; } = string.Empty;
        public LessonType Type { get; set; } = LessonType.Theory;
        public int Order { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
