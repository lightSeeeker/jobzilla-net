using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Models.CandidateDash;

public class CandidateDashAppliedJobsViewModel
{
    public PagedResult<AppliedJobDto> AppliedJobs { get; set; } = null!;
}
