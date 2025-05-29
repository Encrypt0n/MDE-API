using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using MDE_API.Controllers;
using MDE_API.Domain.Models;
using MDE_API.Application.Interfaces;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserController>>();
        _controller = new UserController(_mockService.Object);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new List<User>
    {
        new User { UserID = 1, Username = "Alice", CompanyID = 1 },
        new User { UserID = 2, Username = "Bob", CompanyID = 2 }
    };

        _mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var actionResult = await _controller.GetAllUsers();

        // Assert
        var result = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnUsers = Assert.IsType<List<User>>(result.Value);
        Assert.Equal(2, returnUsers.Count);
    }

    [Fact]
    public async Task GetUserById_ValidId_ReturnsUser()
    {
        // Arrange
        var user = new User { UserID = 1, Username = "Alice", CompanyID = 1 };
        _mockService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var actionResult = await _controller.GetUserById(1);

        // Assert
        var result = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnUser = Assert.IsType<User>(result.Value);
        Assert.Equal("Alice", returnUser.Username);
    }

    [Fact]
    public async Task GetUserById_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetUserByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var actionResult = await _controller.GetUserById(999);

        // Assert
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }





}
