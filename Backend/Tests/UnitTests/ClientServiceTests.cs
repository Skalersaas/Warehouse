using Application.DTOs.Client;
using Application.Services;
using Domain.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Xunit;

namespace Tests.UnitTests;

public class ClientServiceTests : IDisposable
{
    private readonly ApplicationContext _context;
    private readonly Mock<IArchivableRepository<Client>> _mockClientRepository;
    private readonly Mock<ILogger<ClientService>> _mockLogger;
    private readonly ClientService _clientService;

    public ClientServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);
        _mockClientRepository = new Mock<IArchivableRepository<Client>>();
        _mockLogger = new Mock<ILogger<ClientService>>();
        
        _clientService = new ClientService(_mockClientRepository.Object, _context, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidClient_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateClientDto
        {
            Name = "New Client",
            Address = "123 New Street"
        };

        var createdClient = new Client
        {
            Id = 1,
            Name = dto.Name,
            Address = dto.Address,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockClientRepository
            .Setup(x => x.CreateAsync(It.IsAny<Client>()))
            .ReturnsAsync(createdClient);

        // Act
        var result = await _clientService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(dto.Name);
        result.Data.Address.Should().Be(dto.Address);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsFailure()
    {
        // Arrange
        var existingClient = new Client
        {
            Id = 1,
            Name = "Existing Client",
            Address = "123 Existing Street",
            IsArchived = false
        };

        _context.Clients.Add(existingClient);
        await _context.SaveChangesAsync();

        var dto = new CreateClientDto
        {
            Name = "Existing Client", // Same name
            Address = "456 Different Street"
        };

        // Act
        var result = await _clientService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task ExistsByNameAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        var client = new Client
        {
            Id = 1,
            Name = "Test Client",
            Address = "123 Test Street",
            IsArchived = false
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _clientService.ExistsByNameAsync("Test Client");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_NonExistingName_ReturnsFalse()
    {
        // Act
        var result = await _clientService.ExistsByNameAsync("Non-existing Client");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_ExistingNameWithExclusion_ReturnsFalse()
    {
        // Arrange
        var client = new Client
        {
            Id = 1,
            Name = "Test Client",
            Address = "123 Test Street",
            IsArchived = false
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _clientService.ExistsByNameAsync("Test Client", excludeId: 1);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
