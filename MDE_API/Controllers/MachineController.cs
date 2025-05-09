using Microsoft.AspNetCore.Mvc;

namespace MDE_API.Controllers
{
    public class MachineController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
