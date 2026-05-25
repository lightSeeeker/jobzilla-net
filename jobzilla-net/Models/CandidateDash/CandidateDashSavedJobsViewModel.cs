using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Models.CandidateDash;

public class CandidateDashSavedJobsViewModel
{
    public PagedResult<SavedJobDto> SavedJobs { get; set; } = null!;
}
