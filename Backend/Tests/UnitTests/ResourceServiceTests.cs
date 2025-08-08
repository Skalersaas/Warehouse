using Application.DTOs.Resource;
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

public class ResourceServiceTests : IDisposable
{
    private readonly ApplicationContext _context;
    private readonly Mock<IArchivableRepository<Resource>> _mockResourceRepository;
    private readonly Mock<ILogger<ResourceService>> _mockLogger;
    private readonly ResourceService _resourceService;

    public ResourceServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);
        _mockResourceRepository = new Mock<IArchivableRepository<Resource>>();
        _mockLogger = new Mock<ILogger<ResourceService>>();
        
        _resourceService = new ResourceService(_mockResourceRepository.Object, _context, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidResource_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateResourceDto
        {
            Name = "New Resource"
        };

        var createdResource = new Resource
        {
            Id = 1,
            Name = dto.Name,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockResourceRepository
            .Setup(x => x.CreateAsync(It.IsAny<Resource>()))
            .ReturnsAsync(createdResource);

        // Act
        var result = await _resourceService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(dto.Name);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsFailure()
    {
        // Arrange
        var existingResource = new Resource
        {
            Id = 1,
            Name = "Existing Resource",
            IsArchived = false
        };

        _context.Resources.Add(existingResource);
        await _context.SaveChangesAsync();

        var dto = new CreateResourceDto
        {
            Name = "Existing Resource" // Same name
        };

        // Act
        var result = await _resourceService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task ExistsByNameAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        var resource = new Resource
        {
            Id = 1,
            Name = "Test Resource",
            IsArchived = false
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceService.ExistsByNameAsync("Test Resource");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_NonExistingName_ReturnsFalse()
    {
        // Act
        var result = await _resourceService.ExistsByNameAsync("Non-existing Resource");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_ExistingNameWithExclusion_ReturnsFalse()
    {
        // Arrange
        var resource = new Resource
        {
            Id = 1,
            Name = "Test Resource",
            IsArchived = false
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceService.ExistsByNameAsync("Test Resource", excludeId: 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsInUseAsync_ResourceInUse_ReturnsTrue()
    {
        // Arrange
        var resource = new Resource { Id = 1, Name = "Test Resource", IsArchived = false };
        var unit = new Unit { Id = 1, Name = "kg", IsArchived = false };
        var receiptItem = new ReceiptItem 
        { 
            Id = 1, 
            ResourceId = 1, 
            UnitId = 1, 
            Quantity = 10, 
            DocumentId = 1 
        };

        _context.Resources.Add(resource);
        _context.Units.Add(unit);
        _context.ReceiptItems.Add(receiptItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceService.IsInUseAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsInUseAsync_ResourceNotInUse_ReturnsFalse()
    {
        // Arrange
        var resource = new Resource { Id = 1, Name = "Test Resource", IsArchived = false };
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceService.IsInUseAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingResource_ReturnsSuccess()
    {
        // Arrange
        var resource = new Resource
        {
            Id = 1,
            Name = "Test Resource",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockResourceRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(resource);

        // Act
        var result = await _resourceService.GetByIdAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Test Resource");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingResource_ReturnsFailure()
    {
        // Arrange
        _mockResourceRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Resource?)null);

        // Act
        var result = await _resourceService.GetByIdAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("Resource not found");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
