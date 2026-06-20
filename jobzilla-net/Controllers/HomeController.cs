using jobzilla_net.Models;
using jobzilla_net.Models.Home;
using jobzilla_net.Infrasture.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace jobzilla_net.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Jobzilla - Job Board";
            ViewData["Description"] = "Jobzilla job board home page.";
            ViewData["Keywords"] = "jobs, job board, careers, employers, candidates";
            ViewData["Author"] = "Jobzilla";
            ViewData["Robots"] = "index, follow";

            var model = new HomePageViewModel();

            // Site Statistics
            model.TotalActiveUsers = await _context.Users.CountAsync();
            model.TotalOpenJobs = await _context.JobPosts.CountAsync();
            model.TotalJobsCompleted = await _context.JobApplications.CountAsync();

            // Banner Statistics
            model.TotalCompanies = await _context.EmployerProfiles.CountAsync();
            model.TotalCountries = 98;
            model.TotalJobsDone = 3000;

            // Job Categories
            model.JobCategories = await _context.JobCategories
                .AsNoTracking()
                .Select(c => new JobCategoryHomeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = "flaticon-dashboard",
                    JobCount = _context.JobPosts.Count(j => j.JobCategoryId == c.Id)
                })
                .OrderBy(c => c.Name)
                .Take(20)
                .ToListAsync();

            // Top Companies
            model.TopCompanies = await _context.EmployerProfiles
                .AsNoTracking()
                .Select(e => new EmployerHomeDto
                {
                    Id = e.Id,
                    CompanyName = e.CompanyName,
                    LogoUrl = "/images/client-logo/default.png"
                })
                .Take(10)
                .ToListAsync();

            // Featured/Recent Jobs
            model.FeaturedJobs = await _context.JobPosts
                .AsNoTracking()
                .OrderByDescending(j => j.CreatedAtUtc)
                .Select(j => new JobHomeDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = "Company Name",
                    CompanyLogoUrl = "/images/client-logo/default.png",
                    Location = j.Location ?? "Remote",
                    SalaryMin = 0,
                    SalaryMax = 0,
                    JobType = "Full-time",
                    Description = "",
                    PostedDate = j.CreatedAtUtc
                })
                .Take(5)
                .ToListAsync();

            // Testimonials - empty for now
            model.Testimonials = new List<TestimonialHomeDto>();

            // Recent Blog Posts
            model.RecentBlogs = await _context.BlogPosts
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedAtUtc)
                .Select(b => new BlogHomeDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = "",
                    FeaturedImageUrl = "/images/blog/default.jpg",
                    AuthorName = b.AuthorName ?? "Admin",
                    CategoryName = "General",
                    PublishedDate = b.CreatedAtUtc
                })
                .Take(6)
                .ToListAsync();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Faq()
        {
            return View();
        }

        public IActionResult ComingSoon()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
