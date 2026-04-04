using CodeClash.Web.ApiClients.Models.Enums;
using CodeClash.Web.ApiClients.Models.Requests;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace CodeClash.Web.Components.Pages.Courses;

public partial class CreateCourse
{
    private int _step;
    private bool _saving;
    private string _tagInput = string.Empty;

    private readonly string[] _steps = ["Basic Info", "Technologies", "Tags", "Review"];
    private readonly CourseFormModel _form = new();

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("Home", href: "/", icon: Icons.Material.Filled.Home),
        new("Courses", href: "/courses", icon: Icons.Material.Filled.School),
        new("Create Course", href: null, disabled: true)
    ];

    private bool CanProceed() => _step switch
    {
        0 => !string.IsNullOrWhiteSpace(_form.Title) && !string.IsNullOrWhiteSpace(_form.Description),
        _ => true
    };

    private void NextStep() { if (_step < _steps.Length - 1) _step++; }
    private void PrevStep() { if (_step > 0) _step--; }

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

    private void RemoveTag(string tag) => _form.Tags.Remove(tag);

    private void OnTagKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or ",") AddTag();
    }

    private async Task Submit()
    {
        if (string.IsNullOrWhiteSpace(_form.Title) || string.IsNullOrWhiteSpace(_form.Description))
        {
            Snackbar.Add("Title and description are required", Severity.Warning);
            _step = 0;
            return;
        }

        _saving = true;
        try
        {
            var request = new CreateCourseRequest(
                _form.Title,
                _form.Description,
                _form.CodingTechnologies,
                _form.Difficulty,
                _form.Tags,
                string.IsNullOrEmpty(_form.ThumbnailUrl) ? null : _form.ThumbnailUrl);

            var result = await CoursesApi.CreateCourseAsync(request);
            if (result is not null)
            {
                if (_form.IsPublished)
                    await CoursesApi.UpdateCourseAsync(result.Id,
                        new UpdateCourseRequest(null, null, null, null, null, null, true));

                Snackbar.Add("Course created successfully!", Severity.Success);
                Navigation.NavigateTo($"/courses/{result.Id}");
            }
        }
        catch
        {
            Snackbar.Add("Failed to create course. Please try again.", Severity.Error);
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

    private class CourseFormModel
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
