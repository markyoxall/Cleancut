using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CleanCut.WebApp.Models;

namespace CleanCut.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Home page accessed");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Diagnostic()
    {
        _logger.LogInformation("?? Diagnostic page accessed for troubleshooting MVC navigation");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
