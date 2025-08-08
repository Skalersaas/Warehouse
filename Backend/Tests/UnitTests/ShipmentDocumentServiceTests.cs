using Application.DTOs.ShipmentDocument;
using Application.DTOs.ShipmentItem;
using Application.Interfaces;
using Application.Services;
using Domain.Models.Entities;
using Domain.Models.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Xunit;

namespace Tests.UnitTests;

public class ShipmentDocumentServiceTests : IDisposable
{
    private readonly ApplicationContext _context;
    private readonly Mock<IRepository<ShipmentDocument>> _mockShipmentRepository;
    private readonly Mock<IBalanceService> _mockBalanceService;
    private readonly Mock<ILogger<ShipmentDocumentService>> _mockLogger;
    private readonly ShipmentDocumentService _shipmentService;

    public ShipmentDocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);
        _mockShipmentRepository = new Mock<IRepository<ShipmentDocument>>();
        _mockBalanceService = new Mock<IBalanceService>();
        _mockLogger = new Mock<ILogger<ShipmentDocumentService>>();
        
        _shipmentService = new ShipmentDocumentService(
            _mockShipmentRepository.Object, 
            _mockBalanceService.Object, 
            _context, 
            _mockLogger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var unit = new Unit { Id = 1, Name = "kg", IsArchived = false };
        var resource = new Resource { Id = 1, Name = "Steel", IsArchived = false };
        var client = new Client { Id = 1, Name = "Test Client", Address = "123 Test St", IsArchived = false };

        _context.Units.Add(unit);
        _context.Resources.Add(resource);
        _context.Clients.Add(client);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_ValidShipment_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateShipmentDocumentDto
        {
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };

        var createdShipment = new ShipmentDocument
        {
            Id = 1,
            Number = dto.Number,
            ClientId = dto.ClientId,
            Date = dto.Date,
            Status = ShipmentStatus.Draft,
            Items = new List<ShipmentItem>
            {
                new() { Id = 1, ResourceId = 1, UnitId = 1, Quantity = 50, DocumentId = 1 }
            },
            CreatedAt = DateTime.UtcNow
        };

        _mockShipmentRepository
            .Setup(x => x.CreateAsync(It.IsAny<ShipmentDocument>()))
            .ReturnsAsync(createdShipment);

        // Act
        var result = await _shipmentService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Number.Should().Be(dto.Number);
        result.Data.Status.Should().Be(ShipmentStatus.Draft);
        result.Data.Items.Should().HaveCount(1);
        result.ErrorMessage.Should().BeNull();

        // Verify balance service was NOT called on creation (only on signing)
        _mockBalanceService.Verify(x => x.UpdateBalanceOnShipmentSignAsync(It.IsAny<ShipmentDocument>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNumber_ReturnsFailure()
    {
        // Arrange
        var existingShipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Draft
        };

        _context.ShipmentDocuments.Add(existingShipment);
        await _context.SaveChangesAsync();

        var dto = new CreateShipmentDocumentDto
        {
            Number = "SHP-001", // Same number
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };

        // Act
        var result = await _shipmentService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateAsync_InvalidClient_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateShipmentDocumentDto
        {
            Number = "SHP-002",
            ClientId = 999, // Non-existing client
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };

        // Act
        var result = await _shipmentService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("Client not found");
    }

    [Fact]
    public async Task CreateAsync_EmptyItems_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateShipmentDocumentDto
        {
            Number = "SHP-003",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>() // Empty items
        };

        // Act
        var result = await _shipmentService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("at least one item");
    }

    [Fact]
    public async Task SignAsync_DraftShipment_WithSufficientBalance_ReturnsSuccess()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Draft,
            Items = new List<ShipmentItem>
            {
                new() 
                { 
                    Id = 1, 
                    ResourceId = 1, 
                    UnitId = 1, 
                    Quantity = 50, 
                    DocumentId = 1,
                    Resource = new Resource { Id = 1, Name = "Steel" },
                    Unit = new Unit { Id = 1, Name = "kg" }
                }
            }
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        _mockShipmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ShipmentDocument>()))
            .ReturnsAsync((ShipmentDocument s) => s);

        // Act
        var result = await _shipmentService.SignAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();

        // Verify balance validation and update were called
        _mockBalanceService.Verify(x => x.ValidateShipmentBalanceAsync(It.IsAny<ShipmentDocument>()), Times.Once);
        _mockBalanceService.Verify(x => x.UpdateBalanceOnShipmentSignAsync(It.IsAny<ShipmentDocument>()), Times.Once);
    }

    [Fact]
    public async Task SignAsync_DraftShipment_WithInsufficientBalance_ReturnsFailure()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Draft,
            Items = new List<ShipmentItem>
            {
                new() 
                { 
                    Id = 1, 
                    ResourceId = 1, 
                    UnitId = 1, 
                    Quantity = 150, 
                    DocumentId = 1,
                    Resource = new Resource { Id = 1, Name = "Steel" },
                    Unit = new Unit { Id = 1, Name = "kg" }
                }
            }
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        _mockBalanceService
            .Setup(x => x.ValidateShipmentBalanceAsync(It.IsAny<ShipmentDocument>()))
            .ThrowsAsync(new InvalidOperationException("Insufficient balance for Steel"));

        // Act
        var result = await _shipmentService.SignAsync(1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Insufficient balance for Steel");

        // Verify balance update was NOT called
        _mockBalanceService.Verify(x => x.UpdateBalanceOnShipmentSignAsync(It.IsAny<ShipmentDocument>()), Times.Never);
    }

    [Fact]
    public async Task SignAsync_AlreadySignedShipment_ReturnsFailure()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Signed // Already signed
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _shipmentService.SignAsync(1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already signed");
    }

    [Fact]
    public async Task RevokeAsync_SignedShipment_ReturnsSuccess()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Signed,
            Items = new List<ShipmentItem>
            {
                new() { Id = 1, ResourceId = 1, UnitId = 1, Quantity = 50, DocumentId = 1 }
            }
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        _mockShipmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ShipmentDocument>()))
            .ReturnsAsync((ShipmentDocument s) => s);

        // Act
        var result = await _shipmentService.RevokeAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();

        // Verify balance restoration was called
        _mockBalanceService.Verify(x => x.UpdateBalanceOnShipmentRevokeAsync(It.IsAny<ShipmentDocument>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAsync_DraftShipment_ReturnsFailure()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Draft // Not signed
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _shipmentService.RevokeAsync(1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Only signed shipment documents can be revoked");
    }

    [Fact]
    public async Task DeleteAsync_SignedShipment_ReturnsFailure()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Signed
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _shipmentService.DeleteAsync(1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Cannot delete signed shipment documents");
    }

    [Fact]
    public async Task DeleteAsync_DraftShipment_ReturnsSuccess()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Draft
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        _mockShipmentRepository
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _shipmentService.DeleteAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_SignedShipment_ReturnsFailure()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Signed
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateShipmentDocumentDto
        {
            Id = 1,
            Number = "SHP-001-UPDATED",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<UpdateShipmentItemDto>
            {
                new() { Id = 1, ResourceId = 1, UnitId = 1, Quantity = 100 }
            }
        };

        // Act
        var result = await _shipmentService.UpdateAsync(updateDto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Cannot edit signed shipment documents");
    }

    [Fact]
    public async Task ExistsByNumberAsync_ExistingNumber_ReturnsTrue()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Number = "SHP-001",
            ClientId = 1,
            Date = DateTime.Today,
            Status = ShipmentStatus.Draft
        };

        _context.ShipmentDocuments.Add(shipment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _shipmentService.ExistsByNumberAsync("SHP-001");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNumberAsync_NonExistingNumber_ReturnsFalse()
    {
        // Act
        var result = await _shipmentService.ExistsByNumberAsync("SHP-999");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
