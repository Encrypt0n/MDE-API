using MDE_API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/machines")]
    public class MachineController : ControllerBase
    {
      

        private readonly IMachineService _machineService;
        private readonly ILogger<MachineController> _logger;

        public MachineController(IMachineService machineService, ILogger<MachineController> logger)
        {
            _machineService = machineService;
            _logger = logger;
        }

        [HttpGet("company/{companyId}")]
        public IActionResult GetMachinesForUser(int companyId)
        {
            if (companyId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var machines = _machineService.GetMachinesForCompany(companyId);
            foreach (var machine in machines)
            {

                _logger.LogInformation("👤 Name: {Name}", machine.Name);
            }
            return Ok(machines);
        }

        [HttpPost("{machineId}/dashboard-url")]
        public IActionResult UpdateDashboardUrl(int machineId, [FromBody] DashboardUrlUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(model.DashboardUrl))
                return BadRequest("Dashboard URL is required.");

            _machineService.UpdateDashboardUrl(machineId, model.DashboardUrl);
            return Ok("Dashboard URL updated successfully.");
        }

        public class DashboardUrlUpdateModel
        {
            public string DashboardUrl { get; set; }
        }


        [HttpGet("{machineId}")]
        public IActionResult GetMachineById(int machineId)
        {
            if (machineId <= 0)
            {
                return BadRequest("Invalid machine ID.");
            }

            var machine = _machineService.GetMachineById(machineId);

            if (machine == null)
            {
                return NotFound($"Machine with ID {machineId} not found.");
            }

            return Ok(machine);
        }


    }
}
