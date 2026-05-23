using Jobzilla.Web.ViewModels.Modules;
using Microsoft.AspNetCore.Mvc;

namespace Jobzilla.Web.Controllers;

public class ModulesController : Controller
{
    private readonly IWebHostEnvironment _environment;

    public ModulesController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IActionResult Index()
    {
        var templatePath = Path.Combine(_environment.WebRootPath, "template");
        if (!Directory.Exists(templatePath))
        {
            return View(new ModulesIndexViewModel());
        }

        var modules = Directory
            .GetFiles(templatePath, "*.html", SearchOption.TopDirectoryOnly)
            .Select(file =>
            {
                var fileName = Path.GetFileName(file);
                return new TemplateModuleViewModel
                {
                    FileName = fileName,
                    Title = ToTitle(fileName),
                    Category = GetCategory(fileName),
                    Url = $"/template/{fileName}"
                };
            })
            .OrderBy(module => module.Category)
            .ThenBy(module => module.Title)
            .ToList();

        return View(new ModulesIndexViewModel { Modules = modules });
    }

    private static string ToTitle(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName).Replace("-", " ");
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
    }

    private static string GetCategory(string fileName)
    {
        if (fileName.StartsWith("candidate-", StringComparison.OrdinalIgnoreCase))
        {
            return "Candidate";
        }

        if (fileName.StartsWith("employer-", StringComparison.OrdinalIgnoreCase))
        {
            return "Employer";
        }

        if (fileName.StartsWith("dash-", StringComparison.OrdinalIgnoreCase) || fileName.Equals("dashboard.html", StringComparison.OrdinalIgnoreCase))
        {
            return "Dashboard";
        }

        if (fileName.StartsWith("job-", StringComparison.OrdinalIgnoreCase) || fileName.Equals("apply-job.html", StringComparison.OrdinalIgnoreCase))
        {
            return "Jobs";
        }

        if (fileName.StartsWith("blog", StringComparison.OrdinalIgnoreCase))
        {
            return "Blog";
        }

        return "General";
    }
}
