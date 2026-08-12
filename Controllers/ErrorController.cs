using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers;

[AllowAnonymous]
public sealed class ErrorController : Controller
{
    [HttpGet("/Error/404")]
    public IActionResult NotFoundPage()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        Response.Headers.CacheControl = "no-store";
        return View("NotFound");
    }

    [HttpGet("/Error/500")]
    public IActionResult ServerError()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        Response.Headers.CacheControl = "no-store";
        return View("ServerError");
    }

    [HttpGet("/Error/429")]
    public IActionResult TooManyRequests(string? context)
    {
        Response.StatusCode = StatusCodes.Status429TooManyRequests;
        Response.Headers.CacheControl = "no-store";
        ViewData["Context"] = context;
        return View("TooManyRequests");
    }
}
