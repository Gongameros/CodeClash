using CodeClash.Web.ApiClients.Models.Enums;
using Microsoft.AspNetCore.Components;

namespace CodeClash.Web.Components.Shared;

public partial class TechBadge
{
    [Parameter, EditorRequired] public CodingTechnology Technology { get; set; }

    private string GetDisplayName() => Technology switch
    {
        CodingTechnology.CSharp => "C#",
        CodingTechnology.CPlusPlus => "C++",
        CodingTechnology.FSharp => "F#",
        CodingTechnology.JavaScript => "JS",
        CodingTechnology.TypeScript => "TS",
        CodingTechnology.VisualBasic => "VB",
        CodingTechnology.ObjectiveC => "ObjC",
        _ => Technology.ToString()
    };
}
