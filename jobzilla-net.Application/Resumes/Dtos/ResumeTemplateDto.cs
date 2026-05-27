namespace jobzilla_net.Application.Resumes.Dtos;

public class ResumeTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateFilePath { get; set; } = string.Empty;
    public string? PreviewImagePath { get; set; }
}
