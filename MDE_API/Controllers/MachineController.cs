using MDE_API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("user/{userId}")]
        public IActionResult GetMachinesForUser(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var machines = _machineService.GetMachinesForUser(userId);
            return Ok(machines);
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
