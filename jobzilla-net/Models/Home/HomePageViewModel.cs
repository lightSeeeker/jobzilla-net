namespace jobzilla_net.Models.Home;

public class HomePageViewModel
{
    // Banner Statistics
    public int TotalCompanies { get; set; }
    public int TotalCountries { get; set; }
    public int TotalJobsDone { get; set; }

    // Site Statistics
    public int TotalActiveUsers { get; set; }
    public int TotalOpenJobs { get; set; }
    public int TotalJobsCompleted { get; set; }

    // Collections
    public List<JobCategoryHomeDto> JobCategories { get; set; } = new();
    public List<EmployerHomeDto> TopCompanies { get; set; } = new();
    public List<JobHomeDto> FeaturedJobs { get; set; } = new();
    public List<TestimonialHomeDto> Testimonials { get; set; } = new();
    public List<BlogHomeDto> RecentBlogs { get; set; } = new();
}

public class JobCategoryHomeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int JobCount { get; set; }
}

public class EmployerHomeDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
}

public class JobHomeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyLogoUrl { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PostedDate { get; set; }
}

public class TestimonialHomeDto
{
    public int Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorTitle { get; set; } = string.Empty;
    public string AuthorImageUrl { get; set; } = string.Empty;
}

public class BlogHomeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FeaturedImageUrl { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
}
