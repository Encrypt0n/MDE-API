using Xunit;
using Moq;
using System.Data;
using MDE_API.Infrastructure.Repositories;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _mockFactory;
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<IDbCommand> _mockCommand;
    private readonly Mock<IDataReader> _mockReader;
    private readonly Mock<IDataParameterCollection> _mockParameters;

    public UserRepositoryTests()
    {
        _mockFactory = new Mock<IDbConnectionFactory>();
        _mockConnection = new Mock<IDbConnection>();
        _mockCommand = new Mock<IDbCommand>();
        _mockReader = new Mock<IDataReader>();
        _mockParameters = new Mock<IDataParameterCollection>();

        _mockFactory.Setup(f => f.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
        _mockConnection.Setup(c => c.Open());

        // Setup Parameters to be a collection that accepts Add
        var parametersList = new List<IDbDataParameter>();
        _mockParameters.Setup(p => p.Add(It.IsAny<object>())).Callback<object>(param => parametersList.Add((IDbDataParameter)param));
        _mockCommand.SetupGet(c => c.Parameters).Returns(_mockParameters.Object);

        // Setup CreateParameter to return a fresh mock IDbDataParameter every time
        _mockCommand.Setup(c => c.CreateParameter())
            .Returns(() =>
            {
                var mockParam = new Mock<IDbDataParameter>();
                mockParam.SetupAllProperties(); // allows setting ParameterName and Value
                return mockParam.Object;
            });
    }

    [Fact]
    public void UserExists_ShouldReturnTrue_WhenUserExists()
    {
        _mockCommand.Setup(c => c.ExecuteScalar()).Returns(1);

        var repo = new UserRepository(_mockFactory.Object);
        var result = repo.UserExists("existingUser");

        Assert.True(result);
    }

    [Fact]
    public void UserExists_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        _mockCommand.Setup(c => c.ExecuteScalar()).Returns(0);

        var repo = new UserRepository(_mockFactory.Object);
        var result = repo.UserExists("nonExistingUser");

        Assert.False(result);
    }

    [Fact]
    public void CreateUser_ShouldExecuteNonQuery()
    {
        _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

        var repo = new UserRepository(_mockFactory.Object);
        repo.CreateUser("user", "hash", 1);

        _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
    }

    [Fact]
    public void GetUserWithPasswordHash_ShouldReturnUser_WhenUserFound()
    {
        _mockReader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);

        _mockReader.Setup(r => r.GetInt32(0)).Returns(1);
        _mockReader.Setup(r => r.GetString(1)).Returns("hash");
        _mockReader.Setup(r => r.GetInt32(2)).Returns(0);
        _mockReader.Setup(r => r.GetInt32(3)).Returns(1);

        _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

        var repo = new UserRepository(_mockFactory.Object);
        var result = repo.GetUserWithPasswordHash("user");

        Assert.NotNull(result);
        Assert.Equal(1, result.UserID);
        Assert.Equal("hash", result.PasswordHash);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnListOfUsers()
    {
        _mockReader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);

        _mockReader.Setup(r => r.GetInt32(0)).Returns(1);
        _mockReader.Setup(r => r.GetString(1)).Returns("testuser");
        _mockReader.Setup(r => r.GetInt32(2)).Returns(1);

        _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

        var repo = new UserRepository(_mockFactory.Object);
        var result = await repo.GetAllAsync();

        Assert.Single(result);
    }
}
