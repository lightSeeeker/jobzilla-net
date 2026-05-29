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
    public ResumeFieldType FieldType { get; set; } = ResumeFieldType.Text;
}

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum ResumeFieldType
{
    Text,
    Textarea,
    Date,
    Url,
    Email,
    Phone,
    Checkbox
}
