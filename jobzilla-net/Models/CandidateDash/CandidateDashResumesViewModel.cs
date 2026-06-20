using jobzilla_net.Application.Candidates.Dtos;

namespace jobzilla_net.Models.Candidate;

public class CandidateResumesViewModel
{
    public List<CandidateResumeDto> Resumes { get; set; } = new();
}
