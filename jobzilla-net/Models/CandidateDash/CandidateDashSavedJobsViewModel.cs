using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Models.Candidate;

public class CandidateSavedJobsViewModel
{
    public PagedResult<SavedJobDto> SavedJobs { get; set; } = null!;
}
