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

public class BalanceServiceTests : IDisposable
{
    private readonly ApplicationContext _context;
    private readonly Mock<IRepository<Balance>> _mockBalanceRepository;
    private readonly Mock<ILogger<BalanceService>> _mockLogger;
    private readonly BalanceService _balanceService;

    public BalanceServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);
        _mockBalanceRepository = new Mock<IRepository<Balance>>();
        _mockLogger = new Mock<ILogger<BalanceService>>();
        
        _balanceService = new BalanceService(_mockBalanceRepository.Object, _context, _mockLogger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var unit = new Unit { Id = 1, Name = "kg", IsArchived = false };
        var resource = new Resource { Id = 1, Name = "Steel", IsArchived = false };
        var balance = new Balance { Id = 1, ResourceId = 1, UnitId = 1, Quantity = 100 };

        _context.Units.Add(unit);
        _context.Resources.Add(resource);
        _context.Balances.Add(balance);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetCurrentBalanceAsync_ExistingBalance_ReturnsCorrectQuantity()
    {
        // Act
        var result = await _balanceService.GetCurrentBalanceAsync(1, 1);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public async Task GetCurrentBalanceAsync_NonExistingBalance_ReturnsZero()
    {
        // Act
        var result = await _balanceService.GetCurrentBalanceAsync(999, 999);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task HasSufficientBalanceAsync_SufficientBalance_ReturnsTrue()
    {
        // Act
        var result = await _balanceService.HasSufficientBalanceAsync(1, 1, 50);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasSufficientBalanceAsync_InsufficientBalance_ReturnsFalse()
    {
        // Act
        var result = await _balanceService.HasSufficientBalanceAsync(1, 1, 150);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateBalanceOnReceiptAsync_ValidReceipt_UpdatesBalance()
    {
        // Arrange
        var receipt = new ReceiptDocument
        {
            Id = 1,
            Number = "RCP-001",
            Date = DateTime.UtcNow,
            Items = new List<ReceiptItem>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };

        // Act
        await _balanceService.UpdateBalanceOnReceiptAsync(receipt);

        // Assert
        var updatedBalance = await _balanceService.GetCurrentBalanceAsync(1, 1);
        updatedBalance.Should().Be(150); // 100 + 50
    }

    [Fact]
    public async Task ValidateShipmentBalanceAsync_SufficientBalance_DoesNotThrow()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Items = new List<ShipmentItem>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };

        // Act & Assert
        var act = async () => await _balanceService.ValidateShipmentBalanceAsync(shipment);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateShipmentBalanceAsync_InsufficientBalance_ThrowsException()
    {
        // Arrange
        var shipment = new ShipmentDocument
        {
            Id = 1,
            Items = new List<ShipmentItem>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 150 }
            }
        };

        // Act & Assert
        var act = async () => await _balanceService.ValidateShipmentBalanceAsync(shipment);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient balance*");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
