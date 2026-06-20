using jobzilla_net.Application.Candidates.Dtos;
using Microsoft.AspNetCore.Http;

namespace jobzilla_net.Models.Candidate;

public class CandidateProfileViewModel
{
    public CandidateProfileDto Profile { get; set; } = new();
    public IFormFile? ProfileImage { get; set; }
}
