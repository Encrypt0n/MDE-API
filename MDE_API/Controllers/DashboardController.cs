using Microsoft.AspNetCore.Mvc;

namespace MDE_API.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
