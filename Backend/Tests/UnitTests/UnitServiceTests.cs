using Application.DTOs.Unit;
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

public class UnitServiceTests : IDisposable
{
    private readonly ApplicationContext _context;
    private readonly Mock<IArchivableRepository<Unit>> _mockUnitRepository;
    private readonly Mock<ILogger<UnitService>> _mockLogger;
    private readonly UnitService _unitService;

    public UnitServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);
        _mockUnitRepository = new Mock<IArchivableRepository<Unit>>();
        _mockLogger = new Mock<ILogger<UnitService>>();
        
        _unitService = new UnitService(_mockUnitRepository.Object, _context, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidUnit_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateUnitDto
        {
            Name = "pounds"
        };

        var createdUnit = new Unit
        {
            Id = 1,
            Name = dto.Name,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockUnitRepository
            .Setup((IArchivableRepository<Unit> x) => x.CreateAsync(It.IsAny<Unit>()))
            .ReturnsAsync(createdUnit);

        // Act
        var result = await _unitService.CreateAsync(dto);

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
        var existingUnit = new Unit
        {
            Id = 1,
            Name = "kg",
            IsArchived = false
        };

        _context.Units.Add(existingUnit);
        await _context.SaveChangesAsync();

        var dto = new CreateUnitDto
        {
            Name = "kg" // Same name
        };

        // Act
        var result = await _unitService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task ExistsByNameAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        var unit = new Unit
        {
            Id = 1,
            Name = "kg",
            IsArchived = false
        };

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        // Act
        var result = await _unitService.ExistsByNameAsync("kg");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_NonExistingName_ReturnsFalse()
    {
        // Act
        var result = await _unitService.ExistsByNameAsync("grams");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var unit = new Unit
        {
            Id = 1,
            Name = "Kg",
            IsArchived = false
        };

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        // Act
        var result = await _unitService.ExistsByNameAsync("KG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsInUseAsync_UnitInUse_ReturnsTrue()
    {
        // Arrange
        var unit = new Unit { Id = 1, Name = "kg", IsArchived = false };
        var resource = new Resource { Id = 1, Name = "Steel", IsArchived = false };
        var balance = new Balance 
        { 
            Id = 1, 
            ResourceId = 1, 
            UnitId = 1, 
            Quantity = 100 
        };

        _context.Units.Add(unit);
        _context.Resources.Add(resource);
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();

        // Act
        var result = await _unitService.IsInUseAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsInUseAsync_UnitNotInUse_ReturnsFalse()
    {
        // Arrange
        var unit = new Unit { Id = 1, Name = "kg", IsArchived = false };
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        // Act
        var result = await _unitService.IsInUseAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUnit_ReturnsSuccess()
    {
        // Arrange
        var unit = new Unit
        {
            Id = 1,
            Name = "kg",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockUnitRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(unit);

        // Act
        var result = await _unitService.GetByIdAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("kg");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUnit_ReturnsFailure()
    {
        // Arrange
        _mockUnitRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Unit?)null);

        // Act
        var result = await _unitService.GetByIdAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("Unit not found");
    }

    [Fact]
    public async Task UpdateAsync_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var existingUnit = new Unit
        {
            Id = 1,
            Name = "kg",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var updateDto = new UpdateUnitDto
        {
            Id = 1,
            Name = "kilograms",
            IsArchived = false
        };

        var updatedUnit = new Unit
        {
            Id = 1,
            Name = "kilograms",
            IsArchived = false,
            CreatedAt = existingUnit.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUnitRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingUnit);

        _mockUnitRepository
            .Setup((IArchivableRepository<Unit> x) => x.UpdateAsync(It.IsAny<Unit>()))
            .ReturnsAsync(updatedUnit);

        // Act
        var result = await _unitService.UpdateAsync(updateDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("kilograms");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
