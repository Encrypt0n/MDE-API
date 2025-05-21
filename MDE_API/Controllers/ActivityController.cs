using MDE_API.Application.Interfaces;
using MDE_API.Application.Interfaces.MDE_API.Services;
using MDE_API.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/activity")]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpPost]
        public IActionResult LogActivity([FromBody] UserActivityLog activity)
        {
            if (activity == null || activity.MachineId <= 0)
                return BadRequest("Invalid activity log.");

            _activityService.LogActivity(activity);
            return Ok();
        }

        [HttpGet("{machineId}")]
        public IActionResult GetActivityForMachine(int machineId)
        {
            var activities = _activityService.GetActivitiesForMachine(machineId);
            return Ok(activities);
        }
    }
}
