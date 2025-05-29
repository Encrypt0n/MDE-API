using System.Collections.ObjectModel;
using System.Data;
using Moq;
using Xunit;
using MDE_API.Infrastructure.Repositories;
using MDE_API.Domain.Models;
using MDE_API.Application.Interfaces;

public class MachineRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _mockConnectionFactory = new();
    private readonly Mock<IDbConnection> _mockConnection = new();
    private readonly Mock<IDbCommand> _mockCommand = new();
    private readonly Mock<IDataReader> _mockReader = new();

    public MachineRepositoryTests()
    {
        _mockConnectionFactory.Setup(f => f.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
    }

    [Fact]
    public void GetMachinesForCompany_ReturnsMachines()
    {
        // Arrange
        var companyId = 42;

        // Setup parameters collection
        var parameters = new Mock<IDataParameterCollection>();
        _mockCommand.SetupGet(c => c.Parameters).Returns(parameters.Object);

        // Setup CreateParameter to return new SqlParameter mocks
        _mockCommand.Setup(c => c.CreateParameter()).Returns(() =>
        {
            var param = new Mock<IDbDataParameter>();
            param.SetupProperty(p => p.ParameterName);
            param.SetupProperty(p => p.Value);
            return param.Object;
        });

        // Setup CommandText
        _mockCommand.SetupProperty(c => c.CommandText);

        // Setup reader to return two machines then end
        var callCount = 0;
        _mockReader.Setup(r => r.Read()).Returns(() => callCount++ < 2);

        _mockReader.SetupSequence(r => r.GetInt32(0))
            .Returns(1)
            .Returns(2);

        _mockReader.SetupSequence(r => r.GetString(1))
            .Returns("Machine1")
            .Returns("Machine2");

        _mockReader.SetupSequence(r => r.GetString(2))
            .Returns("Desc1")
            .Returns("Desc2");

        _mockReader.SetupSequence(r => r.GetString(3))
            .Returns("192.168.1.1")
            .Returns("192.168.1.2");

        _mockReader.SetupSequence(r => r.GetString(4))
            .Returns("http://dashboard1")
            .Returns("http://dashboard2");

        _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

        var repo = new MachineRepository(_mockConnectionFactory.Object);

        // Act
        var result = repo.GetMachinesForCompany(companyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Machine1", result[0].Name);
        Assert.Equal("Machine2", result[1].Name);
    }

    [Fact]
    public void GetMachineById_ReturnsMachine_WhenFound()
    {
        var machineId = 5;

        var parameters = new Mock<IDataParameterCollection>();
        _mockCommand.SetupGet(c => c.Parameters).Returns(parameters.Object);

        _mockCommand.Setup(c => c.CreateParameter()).Returns(() =>
        {
            var param = new Mock<IDbDataParameter>();
            param.SetupProperty(p => p.ParameterName);
            param.SetupProperty(p => p.Value);
            return param.Object;
        });

        _mockCommand.SetupProperty(c => c.CommandText);

        _mockReader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);

        _mockReader.Setup(r => r.GetInt32(0)).Returns(machineId);
        _mockReader.Setup(r => r.GetString(1)).Returns("MyMachine");
        _mockReader.Setup(r => r.GetString(2)).Returns("Some description");
        _mockReader.Setup(r => r.GetString(3)).Returns("10.0.0.1");
        _mockReader.Setup(r => r.IsDBNull(4)).Returns(false);
        _mockReader.Setup(r => r.GetString(4)).Returns("http://dashboard");

        _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

        var repo = new MachineRepository(_mockConnectionFactory.Object);

        var machine = repo.GetMachineById(machineId);

        Assert.NotNull(machine);
        Assert.Equal(machineId, machine.MachineID);
        Assert.Equal("MyMachine", machine.Name);
        Assert.Equal("Some description", machine.Description);
        Assert.Equal("10.0.0.1", machine.IP);
        Assert.Equal("http://dashboard", machine.DashboardUrl);
    }

    [Fact]
    public void GetMachineById_ReturnsNull_WhenNotFound()
    {
        var machineId = 99;

        var parameters = new Mock<IDataParameterCollection>();
        _mockCommand.SetupGet(c => c.Parameters).Returns(parameters.Object);

        _mockCommand.Setup(c => c.CreateParameter()).Returns(() =>
        {
            var param = new Mock<IDbDataParameter>();
            param.SetupProperty(p => p.ParameterName);
            param.SetupProperty(p => p.Value);
            return param.Object;
        });

        _mockCommand.SetupProperty(c => c.CommandText);

        _mockReader.Setup(r => r.Read()).Returns(false);

        _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

        var repo = new MachineRepository(_mockConnectionFactory.Object);

        var machine = repo.GetMachineById(machineId);

        Assert.Null(machine);
    }

    [Fact]
    public void UpdateDashboardUrl_ExecutesNonQuery()
    {
        var machineId = 3;
        var dashboardUrl = "http://newdashboard";

        var parameters = new Mock<IDataParameterCollection>();
        _mockCommand.SetupGet(c => c.Parameters).Returns(parameters.Object);

        _mockCommand.Setup(c => c.CreateParameter()).Returns(() =>
        {
            var param = new Mock<IDbDataParameter>();
            param.SetupProperty(p => p.ParameterName);
            param.SetupProperty(p => p.Value);
            return param.Object;
        });

        _mockCommand.SetupProperty(c => c.CommandText);

        _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1).Verifiable();

        var repo = new MachineRepository(_mockConnectionFactory.Object);

        repo.UpdateDashboardUrl(machineId, dashboardUrl);

        _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
    }
}
