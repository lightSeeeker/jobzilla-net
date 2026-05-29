namespace jobzilla_net.Application.Resumes.Dtos;

/// <summary>
/// One entry inside a resume section (e.g. a single job, a single degree, a single skill).
/// Fields are dynamic key-value pairs so the model works for any section type without schema changes.
/// </summary>
public class ResumeSectionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int DisplayOrder { get; set; }
    public List<ResumeField> Fields { get; set; } = new();

    // ── Convenience helpers so the JS and templates can access typed values ──

    public string? GetValue(string key) =>
        Fields.FirstOrDefault(f => f.Key.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value;

    public void SetValue(string key, string? value, string? label = null, string type = ResumeFieldType.Text)
    {
        var field = Fields.FirstOrDefault(f => f.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (field != null)
        {
            field.Value = value;
        }
        else
        {
            Fields.Add(new ResumeField { Key = key, Label = label ?? key, Value = value, FieldType = type });
        }
    }
}
