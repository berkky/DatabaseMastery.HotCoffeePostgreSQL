using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
