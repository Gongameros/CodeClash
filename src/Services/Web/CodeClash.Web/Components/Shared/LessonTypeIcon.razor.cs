using CodeClash.Web.ApiClients.Models.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CodeClash.Web.Components.Shared;

public partial class LessonTypeIcon
{
    [Parameter, EditorRequired] public LessonType Type { get; set; }
    [Parameter] public Size Size { get; set; } = Size.Small;

    private string GetIcon() => Type switch
    {
        LessonType.Theory => Icons.Material.Filled.MenuBook,
        LessonType.CodingChallenge => Icons.Material.Filled.Code,
        LessonType.Quiz => Icons.Material.Filled.Quiz,
        _ => Icons.Material.Filled.Help
    };

    private Color GetColor() => Type switch
    {
        LessonType.Theory => Color.Info,
        LessonType.CodingChallenge => Color.Success,
        LessonType.Quiz => Color.Warning,
        _ => Color.Default
    };

    private string GetLabel() => Type switch
    {
        LessonType.Theory => "Theory",
        LessonType.CodingChallenge => "Coding Challenge",
        LessonType.Quiz => "Quiz",
        _ => "Unknown"
    };
}
