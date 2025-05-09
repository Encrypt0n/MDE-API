using Xunit;
using Moq;
using System.Data;
using MDE_API.Application;
using MDE_API.Domain;
using MDE_API.Application.Interfaces;

public class DatabaseServiceTests
{
    [Fact]
    public void RegisterUser_UserDoesNotExist_ReturnsTrue()
    {
        // Arrange
        var mockFactory = new Mock<IDbConnectionFactory>();
        var mockConnection = new Mock<IDbConnection>();
        var mockCheckCmd = new Mock<IDbCommand>();
        var mockInsertCmd = new Mock<IDbCommand>();

        // Setup connection factory
        mockFactory.Setup(f => f.CreateConnection()).Returns(mockConnection.Object);

        // Setup check command
        mockCheckCmd.SetupAllProperties();
        mockCheckCmd.Setup(c => c.ExecuteScalar()).Returns(0);
        mockCheckCmd.Setup(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);

        // Setup insert command
        mockInsertCmd.SetupAllProperties();
        mockInsertCmd.Setup(c => c.ExecuteNonQuery()).Verifiable();
        mockInsertCmd.Setup(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);

        // Setup CreateCommand flow
        int cmdCallCount = 0;
        mockConnection.Setup(c => c.Open());
        mockConnection.Setup(c => c.CreateCommand()).Returns(() =>
        {
            cmdCallCount++;
            return cmdCallCount == 1 ? mockCheckCmd.Object : mockInsertCmd.Object;
        });

        var service = new DatabaseService(mockFactory.Object);

        // Act
        var result = service.RegisterUser("testuser", "testpassword");

        // Assert
        Assert.True(result);
        mockInsertCmd.Verify(c => c.ExecuteNonQuery(), Times.Once);
    }
}
