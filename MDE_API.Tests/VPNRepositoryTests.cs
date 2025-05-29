using Xunit;
using Moq;
using MDE_API.Infrastructure.Repositories;
using MDE_API.Application.Interfaces;
using System.Data;
using System.Collections.Generic;

public class VPNRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _mockFactory;
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<IDbCommand> _mockCommand;
    private readonly Mock<IDataParameterCollection> _mockParameters;

    private readonly VPNRepository _repository;

    public VPNRepositoryTests()
    {
        _mockFactory = new Mock<IDbConnectionFactory>();
        _mockConnection = new Mock<IDbConnection>();
        _mockCommand = new Mock<IDbCommand>();
        _mockParameters = new Mock<IDataParameterCollection>();

        _mockFactory.Setup(f => f.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
        _mockConnection.Setup(c => c.Open());

        _mockCommand.SetupAllProperties();
        _mockCommand.Setup(cmd => cmd.Parameters).Returns(_mockParameters.Object);

        _repository = new VPNRepository(_mockFactory.Object);
    }

    [Fact]
    public void SaveClientConnection_InsertsNewMachineAndLinksCorrectly()
    {
        // Arrange
        var clientName = "Test-PC";
        var description = "My test machine";
        var companyId = 5;
        var ip = "10.0.0.1";
        var urls = new List<string> { "dashboard1", "dashboard2" };

        // Fix: wrap null and int return values in lambdas
        _mockCommand.SetupSequence(cmd => cmd.ExecuteScalar())
    .Returns((object)null!)  // checkCmd.ExecuteScalar()
    .Returns(123);           // insertCmd.ExecuteScalar()


        _mockCommand.Setup(cmd => cmd.ExecuteNonQuery());

        // Act
        _repository.SaveClientConnection(clientName, description, companyId, ip, urls);

        // Assert
        _mockConnection.Verify(c => c.Open(), Times.Once);
        _mockCommand.Verify(cmd => cmd.ExecuteScalar(), Times.Exactly(2));
        _mockCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.AtLeastOnce);
    }


    [Fact]
    public void SaveClientConnection_UpdatesExistingMachine()
    {
        // Arrange
        var clientName = "Existing-PC";
        var description = "Updated description";
        var companyId = 1;
        var ip = "10.0.0.9";
        var urls = new List<string>();

        // Simulate machine found
        _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(42); // Existing MachineID

        _mockCommand.Setup(cmd => cmd.ExecuteNonQuery());

        // Act
        _repository.SaveClientConnection(clientName, description, companyId, ip, urls);

        // Assert
        _mockConnection.Verify(c => c.Open(), Times.Once);
        _mockCommand.Verify(cmd => cmd.ExecuteScalar(), Times.Once);
        _mockCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.AtLeastOnce);
    }
}
