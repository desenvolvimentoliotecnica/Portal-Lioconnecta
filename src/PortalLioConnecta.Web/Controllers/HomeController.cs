using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Web.Models;
using PortalLioConnecta.Web.Services.AntDesign;

namespace PortalLioConnecta.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult CareerTimelineShowcase()
    {
        return View();
    }

    public IActionResult AntDesignShowcase([FromServices] IAntDesignHrShowcaseService showcaseService)
    {
        var model = showcaseService.GetShowcase();
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
