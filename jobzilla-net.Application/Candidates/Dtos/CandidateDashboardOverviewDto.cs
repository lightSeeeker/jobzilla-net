namespace jobzilla_net.Application.Candidates.Dtos;

public class CandidateDashboardOverviewDto
{
    public int AppliedJobsCount { get; set; }
    public int SavedJobsCount { get; set; }
    public int AlertsCount { get; set; }
    public int MessagesCount { get; set; }
    
    // For the recent activities or recent applied jobs widget
    public List<AppliedJobDto> RecentApplications { get; set; } = new();
}
