using CodeClash.Web.ApiClients.Models.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CodeClash.Web.Components.Shared;

public partial class DifficultyBadge
{
    [Parameter, EditorRequired] public CourseDifficulty Difficulty { get; set; }

    private Color GetColor() => Difficulty switch
    {
        CourseDifficulty.Beginner => Color.Success,
        CourseDifficulty.Intermediate => Color.Warning,
        CourseDifficulty.Advanced => Color.Error,
        _ => Color.Default
    };

    private string GetIcon() => Difficulty switch
    {
        CourseDifficulty.Beginner => Icons.Material.Filled.SentimentSatisfied,
        CourseDifficulty.Intermediate => Icons.Material.Filled.SentimentNeutral,
        CourseDifficulty.Advanced => Icons.Material.Filled.LocalFireDepartment,
        _ => Icons.Material.Filled.Help
    };
}
