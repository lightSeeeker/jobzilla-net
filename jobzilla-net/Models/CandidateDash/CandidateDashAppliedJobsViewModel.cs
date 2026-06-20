using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Models.Candidate;

public class CandidateAppliedJobsViewModel
{
    public PagedResult<AppliedJobDto> AppliedJobs { get; set; } = null!;
}
