using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using MDE_API.Controllers;
using MDE_API.Domain;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class AuthControllerTests
{
    private readonly Mock<IJWTService> _mockJwtService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMachineRepository> _mockMachineRepository;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockJwtService = new Mock<IJWTService>();
        _mockUserService = new Mock<IUserService>();
        _mockMachineRepository = new Mock<IMachineRepository>();
        _controller = new AuthController(_mockJwtService.Object, _mockUserService.Object, _mockMachineRepository.Object);
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var model = new LoginModel { Username = "user", Password = "pass" };
        var mockUser = new User { UserID = 1, Username = "user", Role = 5, CompanyID = 1 };

        _mockUserService.Setup(s => s.ValidateUser(model.Username, model.Password))
                        .Returns(mockUser);

        _mockJwtService.Setup(s => s.GenerateToken(mockUser.UserID, mockUser.Role, mockUser.CompanyID))
                       .Returns("fake-jwt-token");

        // Act
        var result = _controller.Login(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tokenObj = okResult.Value;
        var tokenValue = tokenObj?.GetType().GetProperty("token")?.GetValue(tokenObj)?.ToString();

        Assert.Equal("fake-jwt-token", tokenValue);
    }


    [Fact]
    public void Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var model = new LoginModel { Username = "user", Password = "wrongpass" };

        _mockUserService.Setup(s => s.ValidateUser(model.Username, model.Password))
                        .Returns((User)null);

        

        // Act
        var result = _controller.Login(model);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials.", unauthorizedResult.Value);
    }

    [Fact]
    public void Register_ValidInput_ReturnsOk()
    {
        // Arrange
        var model = new RegisterModel { Username = "newuser", Password = "pass", CompanyID = 1 };

        _mockUserService.Setup(s => s.RegisterUser(model.Username, model.Password, model.CompanyID))
                        .Returns(true);

        // Act
        var result = _controller.Register(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User registered successfully.", okResult.Value);
    }

    [Fact]
    public void Register_UsernameExists_ReturnsBadRequest()
    {
        // Arrange
        var model = new RegisterModel { Username = "existinguser", Password = "pass", CompanyID = 1 };

        _mockUserService.Setup(s => s.RegisterUser(model.Username, model.Password, model.CompanyID))
                        .Returns(false);

        // Act
        var result = _controller.Register(model);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username already exists.", badRequest.Value);
    }



    [Fact]
    public void Validate_NoAuthorizationHeader_ReturnsUnauthorized()
    {
        var httpContext = new DefaultHttpContext();
       // httpContext.Request.Headers["Authorization"] = null;

        // Assign HttpContext to the controller
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
        // No Authorization header set
        var result = _controller.Validate(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Validate_AuthorizationHeaderWithoutBearer_ReturnsUnauthorized()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Basic abcdefg";

        // Assign HttpContext to the controller
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
        

        var result = _controller.Validate(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Validate_InvalidToken_ReturnsUnauthorized()
    {
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer invalidtoken";

        // Assign HttpContext to the controller
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        _mockJwtService.Setup(j => j.ValidateToken("invalidtoken")).Returns((ClaimsPrincipal?)null);

        var result = _controller.Validate(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Validate_TokenWithoutNbfClaim_ReturnsUnauthorized()
    {
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "jwt"));
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer validtoken";

        // Assign HttpContext to the controller
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        _mockJwtService.Setup(j => j.ValidateToken("validtoken")).Returns(claimsPrincipal);

        var result = _controller.Validate(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Validate_TokenWithInvalidNbfClaim_ReturnsUnauthorized()
    {
        var claims = new List<Claim> { new Claim("nbf", "notAnInt") };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer validtoken";

        // Assign HttpContext to the controller
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
        

        _mockJwtService.Setup(j => j.ValidateToken("validtoken")).Returns(claimsPrincipal);

        var result = _controller.Validate(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Validate_CompanyDoesNotHaveMachine_ReturnsForbid()
    {
        var claims = new List<Claim> { new Claim("nbf", "123") };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer validtoken";

        // Assign HttpContext to the controller
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        _mockJwtService.Setup(j => j.ValidateToken("validtoken")).Returns(claimsPrincipal);
        _mockMachineRepository.Setup(m => m.GetMachinesForCompany(123)).Returns(new System.Collections.ObjectModel.ObservableCollection<Machine> { new Machine { MachineID = 999 } });

        var result = _controller.Validate(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void Validate_CompanyHasMachine_Returns418StatusCode()
    {
        var claims = new List<Claim> { new Claim("nbf", "123") };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer validtoken";

        // Assign HttpContext to the controller
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        _mockJwtService.Setup(j => j.ValidateToken("validtoken")).Returns(claimsPrincipal);
        _mockMachineRepository.Setup(m => m.GetMachinesForCompany(123)).Returns(new System.Collections.ObjectModel.ObservableCollection<Machine> { new Machine { MachineID = 1 } });

        var result = _controller.Validate(1);

        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(418, statusResult.StatusCode);
    }
}
