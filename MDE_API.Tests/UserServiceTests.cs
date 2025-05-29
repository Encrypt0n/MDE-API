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
    }

}
