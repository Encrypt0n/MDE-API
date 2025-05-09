using Microsoft.AspNetCore.Mvc;

namespace MDE_API.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
