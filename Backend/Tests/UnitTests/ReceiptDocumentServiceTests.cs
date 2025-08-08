using Application.DTOs.ReceiptDocument;
using Application.DTOs.ReceiptItem;
using Application.Interfaces;
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

public class ReceiptDocumentServiceTests : IDisposable
{
    private readonly ApplicationContext _context;
    private readonly Mock<IRepository<ReceiptDocument>> _mockReceiptRepository;
    private readonly Mock<IBalanceService> _mockBalanceService;
    private readonly Mock<ILogger<ReceiptDocumentService>> _mockLogger;
    private readonly ReceiptDocumentService _receiptService;

    public ReceiptDocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);
        _mockReceiptRepository = new Mock<IRepository<ReceiptDocument>>();
        _mockBalanceService = new Mock<IBalanceService>();
        _mockLogger = new Mock<ILogger<ReceiptDocumentService>>();
        
        _receiptService = new ReceiptDocumentService(
            _mockReceiptRepository.Object, 
            _mockBalanceService.Object, 
            _context, 
            _mockLogger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var unit = new Unit { Id = 1, Name = "kg", IsArchived = false };
        var resource = new Resource { Id = 1, Name = "Steel", IsArchived = false };

        _context.Units.Add(unit);
        _context.Resources.Add(resource);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_ValidReceipt_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateReceiptDocumentDto
        {
            Number = "RCP-001",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 100 }
            }
        };

        var createdReceipt = new ReceiptDocument
        {
            Id = 1,
            Number = dto.Number,
            Date = dto.Date,
            Items = new List<ReceiptItem>
            {
                new() { Id = 1, ResourceId = 1, UnitId = 1, Quantity = 100, DocumentId = 1 }
            },
            CreatedAt = DateTime.UtcNow
        };

        _mockReceiptRepository
            .Setup(x => x.CreateAsync(It.IsAny<ReceiptDocument>()))
            .ReturnsAsync(createdReceipt);

        // Act
        var result = await _receiptService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Number.Should().Be(dto.Number);
        result.Data.Items.Should().HaveCount(1);
        result.ErrorMessage.Should().BeNull();

        // Verify balance service was called
        _mockBalanceService.Verify(x => x.UpdateBalanceOnReceiptAsync(It.IsAny<ReceiptDocument>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNumber_ReturnsFailure()
    {
        // Arrange
        var existingReceipt = new ReceiptDocument
        {
            Id = 1,
            Number = "RCP-001",
            Date = DateTime.Today
        };

        _context.ReceiptDocuments.Add(existingReceipt);
        await _context.SaveChangesAsync();

        var dto = new CreateReceiptDocumentDto
        {
            Number = "RCP-001", // Same number
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 100 }
            }
        };

        // Act
        var result = await _receiptService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateAsync_InvalidResource_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateReceiptDocumentDto
        {
            Number = "RCP-002",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 999, UnitId = 1, Quantity = 100 } // Non-existing resource
            }
        };

        // Act
        var result = await _receiptService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("Resource with ID 999 not found");
    }

    [Fact]
    public async Task CreateAsync_InvalidUnit_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateReceiptDocumentDto
        {
            Number = "RCP-003",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 999, Quantity = 100 } // Non-existing unit
            }
        };

        // Act
        var result = await _receiptService.CreateAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("Unit with ID 999 not found");
    }

    [Fact]
    public async Task ExistsByNumberAsync_ExistingNumber_ReturnsTrue()
    {
        // Arrange
        var receipt = new ReceiptDocument
        {
            Id = 1,
            Number = "RCP-001",
            Date = DateTime.Today
        };

        _context.ReceiptDocuments.Add(receipt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _receiptService.ExistsByNumberAsync("RCP-001");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNumberAsync_NonExistingNumber_ReturnsFalse()
    {
        // Act
        var result = await _receiptService.ExistsByNumberAsync("RCP-999");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNumberAsync_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var receipt = new ReceiptDocument
        {
            Id = 1,
            Number = "rcp-001",
            Date = DateTime.Today
        };

        _context.ReceiptDocuments.Add(receipt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _receiptService.ExistsByNumberAsync("RCP-001");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingReceipt_ReturnsSuccess()
    {
        // Arrange
        var receipt = new ReceiptDocument
        {
            Id = 1,
            Number = "RCP-001",
            Date = DateTime.Today,
            Items = new List<ReceiptItem>
            {
                new() 
                { 
                    Id = 1, 
                    ResourceId = 1, 
                    UnitId = 1, 
                    Quantity = 100, 
                    DocumentId = 1,
                    Resource = new Resource { Id = 1, Name = "Steel" },
                    Unit = new Unit { Id = 1, Name = "kg" }
                }
            }
        };

        _context.ReceiptDocuments.Add(receipt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _receiptService.GetByIdAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Number.Should().Be("RCP-001");
        result.Data.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingReceipt_ReturnsFailure()
    {
        // Act
        var result = await _receiptService.GetByIdAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("Receipt document not found");
    }

    [Fact]
    public async Task DeleteAsync_ExistingReceipt_WithSufficientBalance_ReturnsSuccess()
    {
        // Arrange
        var receipt = new ReceiptDocument
        {
            Id = 1,
            Number = "RCP-001",
            Date = DateTime.Today,
            Items = new List<ReceiptItem>
            {
                new() 
                { 
                    Id = 1, 
                    ResourceId = 1, 
                    UnitId = 1, 
                    Quantity = 50, 
                    DocumentId = 1,
                    Resource = new Resource { Id = 1, Name = "Steel" }
                }
            }
        };

        _context.ReceiptDocuments.Add(receipt);
        await _context.SaveChangesAsync();

        // Mock sufficient balance
        _mockBalanceService
            .Setup(x => x.GetCurrentBalanceAsync(1, 1))
            .ReturnsAsync(100); // More than required 50

        _mockReceiptRepository
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _receiptService.DeleteAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();

        // Verify balance service was called
        _mockBalanceService.Verify(x => x.UpdateBalanceOnReceiptDeleteAsync(It.IsAny<ReceiptDocument>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingReceipt_WithInsufficientBalance_ReturnsFailure()
    {
        // Arrange
        var receipt = new ReceiptDocument
        {
            Id = 1,
            Number = "RCP-001",
            Date = DateTime.Today,
            Items = new List<ReceiptItem>
            {
                new() 
                { 
                    Id = 1, 
                    ResourceId = 1, 
                    UnitId = 1, 
                    Quantity = 100, 
                    DocumentId = 1,
                    Resource = new Resource { Id = 1, Name = "Steel" }
                }
            }
        };

        _context.ReceiptDocuments.Add(receipt);
        await _context.SaveChangesAsync();

        // Mock insufficient balance
        _mockBalanceService
            .Setup(x => x.GetCurrentBalanceAsync(1, 1))
            .ReturnsAsync(50); // Less than required 100

        // Act
        var result = await _receiptService.DeleteAsync(1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("insufficient current balance");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
