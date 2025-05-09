using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;
using System.Security.Cryptography;
using MDE_API.Domain;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly DatabaseService _databaseService;

        public AuthController(JwtService jwtService, DatabaseService databaseService)
        {
            _jwtService = jwtService;
            _databaseService = databaseService;
        }

        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (_databaseService.ValidateUser(model.Username, model.Password).Username != model.Username)
                return Unauthorized("Invalid credentials.");

            var token = _jwtService.GenerateToken(model.Username);
            Debug.WriteLine(token);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (!_databaseService.RegisterUser(model.Username, model.Password))
                return BadRequest("Username already exists.");

            return Ok("User registered successfully.");
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
