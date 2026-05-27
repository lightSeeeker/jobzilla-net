namespace jobzilla_net.Application.Resumes.ViewModels;

public class ResumeTemplateViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImagePath { get; set; }
}
