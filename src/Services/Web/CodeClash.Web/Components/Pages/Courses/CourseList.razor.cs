using CodeClash.Web.ApiClients.Models.Enums;
using CodeClash.Web.ApiClients.Models.Responses;

namespace CodeClash.Web.Components.Pages.Courses;

public partial class CourseList
{
    private List<CourseListItem> _allCourses = [];
    private List<CourseListItem> _filteredCourses = [];
    private bool _loading = true;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;
    private const int _pageSize = 9;

    private string _searchText = string.Empty;
    private readonly HashSet<CourseDifficulty> _selectedDifficulties = [];
    private readonly HashSet<CodingTechnology> _selectedTechs = [];

    private static readonly CodingTechnology[] _popularTechs =
    [
        CodingTechnology.CSharp, CodingTechnology.Python, CodingTechnology.JavaScript,
        CodingTechnology.TypeScript, CodingTechnology.Java, CodingTechnology.Rust,
        CodingTechnology.Golang, CodingTechnology.CPlusPlus
    ];

    protected override async Task OnInitializedAsync() => await LoadCoursesAsync();

    private async Task LoadCoursesAsync()
    {
        _loading = true;
        var result = await CoursesApi.GetCoursesAsync(_currentPage, _pageSize);
        _allCourses = result.Items;
        _totalCount = result.TotalCount;
        _totalPages = result.TotalPages;
        ApplyFilters();
        _loading = false;
    }

    private void ApplyFilters()
    {
        _filteredCourses = _allCourses
            .Where(c =>
                (string.IsNullOrEmpty(_searchText) || c.Title.Contains(_searchText, StringComparison.OrdinalIgnoreCase) || c.Description.Contains(_searchText, StringComparison.OrdinalIgnoreCase)) &&
                (_selectedDifficulties.Count == 0 || _selectedDifficulties.Contains(c.Difficulty)) &&
                (_selectedTechs.Count == 0 || c.CodingTechnologies.Any(t => _selectedTechs.Contains(t))))
            .ToList();
    }

    private void ToggleDifficulty(CourseDifficulty diff, bool add)
    {
        if (add) _selectedDifficulties.Add(diff);
        else _selectedDifficulties.Remove(diff);
        ApplyFilters();
    }

    private void ToggleTech(CodingTechnology tech)
    {
        if (!_selectedTechs.Remove(tech)) _selectedTechs.Add(tech);
        ApplyFilters();
    }

    private void ClearFilters()
    {
        _searchText = string.Empty;
        _selectedDifficulties.Clear();
        _selectedTechs.Clear();
        ApplyFilters();
    }

    private async Task OnPageChanged(int page)
    {
        _currentPage = page;
        await LoadCoursesAsync();
    }

    private static string GetTechName(CodingTechnology tech) => tech switch
    {
        CodingTechnology.CSharp => "C#",
        CodingTechnology.CPlusPlus => "C++",
        CodingTechnology.JavaScript => "JS",
        CodingTechnology.TypeScript => "TS",
        _ => tech.ToString()
    };
}
