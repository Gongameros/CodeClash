using CodeClash.Web.ApiClients.Models.Responses;
using Microsoft.AspNetCore.Components;

namespace CodeClash.Web.Components.Shared;

public partial class CourseCard
{
    [Parameter, EditorRequired] public CourseListItem Course { get; set; } = null!;

    private void Navigate() => Navigation.NavigateTo($"/courses/{Course.Id}");

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength] + "…";
}
