using jobzilla_net.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace jobzilla_net.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Jobzilla - Job Board";
            ViewData["Description"] = "Jobzilla job board home page.";
            ViewData["Keywords"] = "jobs, job board, careers, employers, candidates";
            ViewData["Author"] = "Jobzilla";
            ViewData["Robots"] = "index, follow";

            return View();
        }

        public IActionResult Privacy()
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
