using CodeClash.Web.ApiClients.Models.Responses;

namespace CodeClash.Web.Components.Pages;

public partial class Home
{
    private List<CourseListItem> _courses = [];
    private bool _loading = true;
    private StatsModel? _stats;

    protected override async Task OnInitializedAsync()
    {
        var result = await CoursesApi.GetCoursesAsync(page: 1, pageSize: 6);
        _courses = result.Items;
        _stats = new StatsModel(
            result.Items.Count > 0 ? result.TotalCount : 0,
            result.Items.Sum(c => c.EnrolledCount),
            result.Items.Sum(c => c.TotalXp));
        _loading = false;
    }

    private record StatsModel(int TotalCourses, int TotalEnrolled, int TotalXp);
}
