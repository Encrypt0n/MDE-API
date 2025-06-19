using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;
using System.Security.Cryptography;
using MDE_API.Domain;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using MDE_API.Application.Interfaces;
using MDE_API.Infrastructure.Repositories;

namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJWTService _jwtService;
        private readonly IUserService _userService;
        private readonly IMachineRepository _machineRepository; // Assume you have this

        public AuthController(IJWTService jwtService, IUserService userService, IMachineRepository machineRepository)
        {
            _jwtService = jwtService;
            _userService = userService;
            _machineRepository = machineRepository;
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _userService.ValidateUser(model.Username, model.Password);
            if (user == null || user.Username != model.Username)
                return Unauthorized("Invalid credentials.");

            var token = _jwtService.GenerateToken(user.UserID, user.Role, user.CompanyID);
            //Debug.WriteLine(token);
            return Ok(new { token });

        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (!_userService.RegisterUser(model.Username, model.Password, model.CompanyID))
                return BadRequest("Username already exists.");

            return Ok("User registered successfully.");
        }

        [HttpGet("validate")]
        public IActionResult Validate([FromQuery] int machineId)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized();

            var token = authHeader.Substring("Bearer ".Length);
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
                return Unauthorized();

            var companyIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                return Unauthorized();

            // Check if the company has access to the machine
            var machines = _machineRepository.GetMachinesForCompany(companyId);
            if (!machines.Any(m => m.MachineID == machineId))
                return Forbid(); // Not allowed

            return StatusCode(418);

        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int UserID { get; set; }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int CompanyID { get; set; }
    }
}
