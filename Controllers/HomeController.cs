using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DatabaseMastery.HotCoffeePostgreSQL.Models;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Menu");
    }

    public IActionResult Privacy()
    {
        return RedirectToAction("Index", "Menu");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
