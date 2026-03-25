using CodeClash.Web.ApiClients.Models.Requests;
using CodeClash.Web.ApiClients.Models.Responses;
using MudBlazor;

namespace CodeClash.Web.Components.Pages.Courses;

public partial class MyCourses
{
    private List<CourseListItem> _courses = [];
    private bool _loading = true;

    protected override async Task OnInitializedAsync() => await LoadCoursesAsync();

    private async Task LoadCoursesAsync()
    {
        _loading = true;
        var result = await CoursesApi.GetCoursesAsync(page: 1, pageSize: 100);
        _courses = result.Items;
        _loading = false;
    }

    private async Task TogglePublish(CourseListItem course)
    {
        try
        {
            await CoursesApi.UpdateCourseAsync(course.Id,
                new UpdateCourseRequest(null, null, null, null, null, null, !course.IsPublished));
            var msg = course.IsPublished ? "Course unpublished" : "Course published";
            Snackbar.Add(msg, Severity.Success);
            await LoadCoursesAsync();
        }
        catch { Snackbar.Add("Failed to update course", Severity.Error); }
    }

    private async Task DeleteCourse(CourseListItem course)
    {
        var confirmed = await DialogService.ShowMessageBox(
            "Delete Course",
            $"Delete '{course.Title}'? This will also remove all modules and lessons.",
            yesText: "Delete", cancelText: "Cancel");
        if (confirmed != true) return;

        try
        {
            await CoursesApi.DeleteCourseAsync(course.Id);
            Snackbar.Add("Course deleted", Severity.Success);
            await LoadCoursesAsync();
        }
        catch { Snackbar.Add("Failed to delete course", Severity.Error); }
    }

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max] + "…";
}
