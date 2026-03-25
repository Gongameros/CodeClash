using CodeClash.Identity.Extensions;
using CodeClash.Web.ApiClients.Models.Enums;
using CodeClash.Web.ApiClients.Models.Requests;
using CodeClash.Web.ApiClients.Models.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace CodeClash.Web.Components.Pages.Courses;

public partial class EditCourse
{
    [Parameter] public string CourseId { get; set; } = string.Empty;
    [CascadingParameter] private Task<AuthenticationState> AuthState { get; set; } = default!;

    private CourseDetailResponse? _course;
    private bool _loading = true;
    private bool _saving;
    private string _tagInput = string.Empty;

    private readonly EditFormModel _form = new();

    private List<BreadcrumbItem> _breadcrumbs =
    [
        new("Home", href: "/"),
        new("Courses", href: "/courses"),
        new("Loading...", href: null, disabled: true),
        new("Edit", href: null, disabled: true)
    ];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _course = await CoursesApi.GetCourseByIdAsync(CourseId);
            if (_course is not null)
            {
                var authState = await AuthState;
                var userId = authState.User.GetUserId();
                if (userId is null || userId != _course.AuthorId)
                {
                    Navigation.NavigateTo($"/courses/{CourseId}", replace: true);
                    return;
                }

                _form.Title = _course.Title;
                _form.Description = _course.Description;
                _form.Difficulty = _course.Difficulty;
                _form.ThumbnailUrl = _course.ThumbnailUrl ?? string.Empty;
                _form.CodingTechnologies = [.. _course.CodingTechnologies];
                _form.Tags = [.. _course.Tags];
                _form.IsPublished = _course.IsPublished;

                _breadcrumbs[2] = new(_course.Title, href: $"/courses/{CourseId}");
            }
        }
        finally
        {
            _loading = false;
        }
    }

    private void ToggleTech(CodingTechnology tech)
    {
        if (!_form.CodingTechnologies.Remove(tech))
            _form.CodingTechnologies.Add(tech);
    }

    private void AddTag()
    {
        var tag = _tagInput.Trim().ToLower().Replace(" ", "-");
        if (!string.IsNullOrEmpty(tag) && !_form.Tags.Contains(tag))
            _form.Tags.Add(tag);
        _tagInput = string.Empty;
    }

    private void OnTagKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or ",") AddTag();
    }

    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(_form.Title)) return;
        _saving = true;
        try
        {
            await CoursesApi.UpdateCourseAsync(CourseId, new UpdateCourseRequest(
                _form.Title,
                _form.Description,
                _form.CodingTechnologies,
                _form.Difficulty,
                _form.Tags,
                string.IsNullOrEmpty(_form.ThumbnailUrl) ? null : _form.ThumbnailUrl,
                _form.IsPublished));

            Snackbar.Add("Course updated successfully!", Severity.Success);
            Navigation.NavigateTo($"/courses/{CourseId}");
        }
        catch
        {
            Snackbar.Add("Failed to save changes", Severity.Error);
        }
        finally { _saving = false; }
    }

    private static string GetTechName(CodingTechnology tech) => tech switch
    {
        CodingTechnology.CSharp => "C#",
        CodingTechnology.CPlusPlus => "C++",
        CodingTechnology.FSharp => "F#",
        CodingTechnology.JavaScript => "JavaScript",
        CodingTechnology.TypeScript => "TypeScript",
        CodingTechnology.VisualBasic => "Visual Basic",
        CodingTechnology.ObjectiveC => "Objective-C",
        _ => tech.ToString()
    };

    private class EditFormModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CourseDifficulty Difficulty { get; set; } = CourseDifficulty.Beginner;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public List<CodingTechnology> CodingTechnologies { get; set; } = [];
        public List<string> Tags { get; set; } = [];
        public bool IsPublished { get; set; }
    }
}
