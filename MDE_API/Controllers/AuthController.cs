using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;
using System.Security.Cryptography;
using MDE_API.Domain;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using MDE_API.Application.Interfaces;

namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJWTService _jwtService;
        private readonly IUserService _userService;

        public AuthController(IJWTService jwtService, IUserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _userService.ValidateUser(model.Username, model.Password);
            if (user == null || user.Username != model.Username)
                return Unauthorized("Invalid credentials.");

            var token = _jwtService.GenerateToken(user.UserID, user.Role, user.CompanyID);
            Debug.WriteLine(token);
            return Ok(new { token });

        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (!_userService.RegisterUser(model.Username, model.Password, model.CompanyID))
                return BadRequest("Username already exists.");

            return Ok("User registered successfully.");
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
