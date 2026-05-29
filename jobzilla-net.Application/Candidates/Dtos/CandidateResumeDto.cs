namespace jobzilla_net.Application.Candidates.Dtos;

public class CandidateResumeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsBuilderGenerated { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
