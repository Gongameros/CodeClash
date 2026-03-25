using CodeClash.Identity.Extensions;
using CodeClash.Web.ApiClients.Models.Enums;
using CodeClash.Web.ApiClients.Models.Requests;
using CodeClash.Web.ApiClients.Models.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace CodeClash.Web.Components.Pages.Courses;

public partial class CourseDetail
{
    [Parameter] public string CourseId { get; set; } = string.Empty;
    [CascadingParameter] private Task<AuthenticationState> AuthState { get; set; } = default!;

    private CourseDetailResponse? _course;
    private bool _isOwner;
    private bool _loading = true;
    private bool _showAddModule;
    private bool _showAddLesson;
    private bool _saving;
    private string _activeModuleId = string.Empty;

    private readonly ModuleFormModel _moduleForm = new();
    private readonly LessonFormModel _lessonForm = new();

    private List<BreadcrumbItem> _breadcrumbs =
    [
        new("Home", href: "/", icon: Icons.Material.Filled.Home),
        new("Courses", href: "/courses", icon: Icons.Material.Filled.School),
        new("Loading...", href: null, disabled: true)
    ];

    protected override async Task OnInitializedAsync()
    {
        await LoadCourseAsync();
    }

    private async Task LoadCourseAsync()
    {
        _loading = true;
        _course = await CoursesApi.GetCourseByIdAsync(CourseId);

        if (_course is not null)
        {
            _breadcrumbs[2] = new(_course.Title, href: null, disabled: true);
            var authState = await AuthState;
            var userId = authState.User.GetUserId();
            _isOwner = userId is not null && userId == _course.AuthorId;
        }

        _loading = false;
    }

    private void ShowAddModuleDialog()
    {
        _moduleForm.Reset();
        _moduleForm.Order = (_course?.Modules.Count ?? 0);
        _showAddModule = true;
    }

    private void ShowEditModuleDialog(ModuleResponse module)
    {
        _moduleForm.Title = module.Title;
        _moduleForm.Description = module.Description ?? string.Empty;
        _moduleForm.Order = module.Order;
        _moduleForm.XpReward = module.XpReward;
        _moduleForm.EditingModuleId = module.ModuleId;
        _showAddModule = true;
    }

    private void ShowAddLessonDialog(string moduleId)
    {
        _activeModuleId = moduleId;
        _lessonForm.Reset();
        _lessonForm.Order = _course?.Modules.FirstOrDefault(m => m.ModuleId == moduleId)?.Lessons.Count ?? 0;
        _showAddLesson = true;
    }

    private async Task SaveModule()
    {
        if (string.IsNullOrWhiteSpace(_moduleForm.Title)) return;
        _saving = true;
        try
        {
            if (_moduleForm.EditingModuleId is not null)
            {
                await CoursesApi.UpdateModuleAsync(CourseId, _moduleForm.EditingModuleId,
                    new UpdateModuleRequest(_moduleForm.Title, _moduleForm.Description, _moduleForm.Order, _moduleForm.XpReward));
                Snackbar.Add("Module updated", Severity.Success);
            }
            else
            {
                await CoursesApi.AddModuleAsync(CourseId,
                    new AddModuleRequest(_moduleForm.Title, _moduleForm.Description, _moduleForm.Order, _moduleForm.XpReward));
                Snackbar.Add("Module added", Severity.Success);
            }
            _showAddModule = false;
            await LoadCourseAsync();
        }
        catch
        {
            Snackbar.Add("Failed to save module", Severity.Error);
        }
        finally { _saving = false; }
    }

    private async Task SaveLesson()
    {
        if (string.IsNullOrWhiteSpace(_lessonForm.Title)) return;
        _saving = true;
        try
        {
            var content = _lessonForm.Type == LessonType.Theory && !string.IsNullOrWhiteSpace(_lessonForm.Content)
                ? _lessonForm.Content : null;
            var response = await CoursesApi.AddLessonAsync(CourseId, _activeModuleId,
                new AddLessonRequest(_lessonForm.Title, _lessonForm.Type, _lessonForm.Order, content, null));
            Snackbar.Add("Lesson added", Severity.Success);
            _showAddLesson = false;
            await LoadCourseAsync();
            if (response?.LessonId is not null)
                Navigation.NavigateTo($"/courses/{CourseId}/modules/{_activeModuleId}/lessons/{response.LessonId}/edit");
        }
        catch
        {
            Snackbar.Add("Failed to add lesson", Severity.Error);
        }
        finally { _saving = false; }
    }

    private async Task DeleteModule(ModuleResponse module)
    {
        var confirmed = await DialogService.ShowMessageBox("Delete Module",
            $"Are you sure you want to delete '{module.Title}' and all its lessons?",
            yesText: "Delete", cancelText: "Cancel");
        if (confirmed != true) return;

        try
        {
            await CoursesApi.DeleteModuleAsync(CourseId, module.ModuleId);
            Snackbar.Add("Module deleted", Severity.Success);
            await LoadCourseAsync();
        }
        catch { Snackbar.Add("Failed to delete module", Severity.Error); }
    }

    private async Task DeleteLesson(string moduleId, LessonResponse lesson)
    {
        var confirmed = await DialogService.ShowMessageBox("Delete Lesson",
            $"Are you sure you want to delete '{lesson.Title}'?",
            yesText: "Delete", cancelText: "Cancel");
        if (confirmed != true) return;

        try
        {
            await CoursesApi.DeleteLessonAsync(CourseId, moduleId, lesson.LessonId);
            Snackbar.Add("Lesson deleted", Severity.Success);
            await LoadCourseAsync();
        }
        catch { Snackbar.Add("Failed to delete lesson", Severity.Error); }
    }

    private async Task DeleteCourse()
    {
        var confirmed = await DialogService.ShowMessageBox("Delete Course",
            $"Are you sure you want to delete '{_course!.Title}'? This action cannot be undone.",
            yesText: "Delete", cancelText: "Cancel");
        if (confirmed != true) return;

        try
        {
            await CoursesApi.DeleteCourseAsync(CourseId);
            Snackbar.Add("Course deleted", Severity.Success);
            Navigation.NavigateTo("/courses");
        }
        catch { Snackbar.Add("Failed to delete course", Severity.Error); }
    }

    private class ModuleFormModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public int XpReward { get; set; } = 100;
        public string? EditingModuleId { get; set; }

        public void Reset()
        {
            Title = string.Empty;
            Description = string.Empty;
            Order = 0;
            XpReward = 100;
            EditingModuleId = null;
        }
    }

    private class LessonFormModel
    {
        public string Title { get; set; } = string.Empty;
        public LessonType Type { get; set; } = LessonType.Theory;
        public int Order { get; set; }
        public string Content { get; set; } = string.Empty;

        public void Reset()
        {
            Title = string.Empty;
            Type = LessonType.Theory;
            Order = 0;
            Content = string.Empty;
        }
    }
}
