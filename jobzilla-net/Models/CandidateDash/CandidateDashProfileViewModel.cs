using jobzilla_net.Application.Candidates.Dtos;
using Microsoft.AspNetCore.Http;

namespace jobzilla_net.Models.CandidateDash;

public class CandidateDashProfileViewModel
{
    public CandidateProfileDto Profile { get; set; } = new();
    public IFormFile? ProfileImage { get; set; }
}
