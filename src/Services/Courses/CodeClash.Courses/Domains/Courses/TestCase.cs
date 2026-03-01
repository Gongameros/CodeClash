namespace CodeClash.Courses.Domains.Courses;

public class TestCase
{
    public string Input { get; set; } = null!;
    public string ExpectedOutput { get; set; } = null!;
    public bool IsHidden { get; set; }
    public string? Description { get; set; }
}
