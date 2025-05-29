using MDE_API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        


        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("{machineId}")]
        public IActionResult GetDashboardPages(int machineId)
        {
            var pages = _dashboardService.GetDashboardPages(machineId);
            return Ok(pages);
        }

        public class AddDashboardPageModel
        {
            public int MachineId { get; set; }
            public string PageName { get; set; }
            public string PageUrl { get; set; }
        }

        [HttpPost]
        public IActionResult AddDashboardPage([FromBody] AddDashboardPageModel model)
        {
            if (model.MachineId <= 0 || string.IsNullOrWhiteSpace(model.PageName) || string.IsNullOrWhiteSpace(model.PageUrl))
            {
                return BadRequest("Invalid dashboard page data.");
            }

            _dashboardService.AddDashboardPage(model.MachineId, model.PageName, model.PageUrl);
            return Ok();
        }

        [HttpDelete("{pageId}")]
        public IActionResult DeleteDashboardPage(int pageId)
        {
            _dashboardService.DeleteDashboardPage(pageId);
            return Ok();
        }

        [HttpGet("default-url/{machineId}")]
        public IActionResult GetFirstDashboardPageUrl(int machineId)
        {
            var url = _dashboardService.GetFirstDashboardPageUrl(machineId);
            return Ok(url);
        }

    }
}
