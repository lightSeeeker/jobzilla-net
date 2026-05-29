namespace jobzilla_net.Application.Resumes.Dtos;

/// <summary>
/// A single key-value field within a section item.
/// FieldType controls how the UI renders the input (text, date, url, textarea, etc.)
/// </summary>
public class ResumeField
{
    public string Key { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? Value { get; set; }
    public string FieldType { get; set; } = "text";
}

public static class ResumeFieldType
{
    public const string Text = "text";
    public const string Textarea = "textarea";
    public const string Date = "date";
    public const string Url = "url";
    public const string Email = "email";
    public const string Phone = "tel";
    public const string Checkbox = "checkbox";
}
