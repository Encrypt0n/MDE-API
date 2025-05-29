using MDE_API.Application.Interfaces;
using MDE_API.Application.Services;
using MDE_API.Domain.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _service = new UserService(_mockRepo.Object);
        }

        [Fact]
        public void RegisterUser_UserExists_ReturnsFalse()
        {
            _mockRepo.Setup(r => r.UserExists("testuser")).Returns(true);

            var result = _service.RegisterUser("testuser", "password", 1);

            Assert.False(result);
        }

        [Fact]
        public void RegisterUser_NewUser_CreatesUserAndReturnsTrue()
        {
            _mockRepo.Setup(r => r.UserExists("newuser")).Returns(false);

            var result = _service.RegisterUser("newuser", "password", 1);

            _mockRepo.Verify(r => r.CreateUser(It.IsAny<string>(), It.IsAny<string>(), 1), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void ValidateUser_InvalidUsername_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetUserWithPasswordHash("baduser")).Returns((UserWithPasswordHash)null);

            var result = _service.ValidateUser("baduser", "password");

            Assert.Null(result);
        }

        [Fact]
        public void ValidateUser_ValidCredentials_ReturnsUser()
        {
            var userHash = BCrypt.Net.BCrypt.HashPassword("password");
            _mockRepo.Setup(r => r.GetUserWithPasswordHash("user")).Returns(new UserWithPasswordHash
            {
                UserID = 1,
                Role = 2,
                CompanyID = 3,
                PasswordHash = userHash
            });

            var result = _service.ValidateUser("user", "password");

            Assert.NotNull(result);
            Assert.Equal(1, result.UserID);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserID = 1, Username = "user1" },
                new User { UserID = 2, Username = "user2" }
            };

            _mockRepo.Setup(repo => repo.GetAllAsync())
                         .ReturnsAsync(users);

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Collection(result,
                u => Assert.Equal("user1", u.Username),
                u => Assert.Equal("user2", u.Username));
        }

        [Fact]
        public async Task GetUserByIdAsync_UserExists_ReturnsUser()
        {
            // Arrange
            var user = new User { UserID = 1, Username = "user1" };

            _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                         .ReturnsAsync(user);

            // Act
            var result = await _service.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result?.UserID);
            Assert.Equal("user1", result?.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetByIdAsync(99))
                         .ReturnsAsync((User?)null);

            // Act
            var result = await _service.GetUserByIdAsync(99);

            // Assert
            Assert.Null(result);
        }
    }

}
