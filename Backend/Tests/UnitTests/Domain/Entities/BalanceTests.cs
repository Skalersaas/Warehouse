using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Models.Entities;
using Domain.Models.Interfaces;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class BalanceTests
{
    private readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    [Fact]
    public void Balance_ShouldImplementIModel()
    {
        // Arrange & Act
        var balance = new Balance();

        // Assert
        balance.Should().BeAssignableTo<IModel>();
    }

    [Fact]
    public void Balance_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var balance = new Balance();

        // Assert
        balance.Id.Should().Be(0);
        balance.ResourceId.Should().Be(0);
        balance.UnitId.Should().Be(0);
        balance.Quantity.Should().Be(0);
        balance.Resource.Should().BeNull();
        balance.Unit.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void Balance_ShouldSetPropertiesCorrectly(int id, int resourceId, int unitId, decimal quantity)
    {
        // Arrange & Act
        var balance = new Balance
        {
            Id = id,
            ResourceId = resourceId,
            UnitId = unitId,
            Quantity = quantity
        };

        // Assert
        balance.Id.Should().Be(id);
        balance.ResourceId.Should().Be(resourceId);
        balance.UnitId.Should().Be(unitId);
        balance.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void Balance_ShouldSupportNavigationProperties()
    {
        // Arrange
        var balance = new Balance();
        var resource = _fixture.Create<Resource>();
        var unit = _fixture.Create<Unit>();

        // Act
        balance.Resource = resource;
        balance.Unit = unit;

        // Assert
        balance.Resource.Should().BeSameAs(resource);
        balance.Unit.Should().BeSameAs(unit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Balance_ShouldAcceptZeroAndNegativeQuantities(decimal quantity)
    {
        // Arrange & Act
        var balance = new Balance { Quantity = quantity };

        // Assert
        // Note: Business rules about negative balances should be handled at the service layer
        balance.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(1.5)]
    [InlineData(999999.999)]
    public void Balance_ShouldAcceptPositiveQuantities(decimal quantity)
    {
        // Arrange & Act
        var balance = new Balance { Quantity = quantity };

        // Assert
        balance.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void Balance_ShouldSupportForeignKeyAssignment()
    {
        // Arrange
        var balance = new Balance();
        var resourceId = _fixture.Create<int>();
        var unitId = _fixture.Create<int>();

        // Act
        balance.ResourceId = resourceId;
        balance.UnitId = unitId;

        // Assert
        balance.ResourceId.Should().Be(resourceId);
        balance.UnitId.Should().Be(unitId);
    }

    [Fact]
    public void Balance_ShouldAllowZeroForeignKeys()
    {
        // Arrange & Act
        var balance = new Balance
        {
            ResourceId = 0,
            UnitId = 0
        };

        // Assert
        // Note: Foreign key validation should be handled at the database/business logic level
        balance.ResourceId.Should().Be(0);
        balance.UnitId.Should().Be(0);
    }

    [Fact]
    public void Balance_ShouldSupportPreciseDecimalQuantities()
    {
        // Arrange
        var preciseQuantity = 123.456789m;

        // Act
        var balance = new Balance { Quantity = preciseQuantity };

        // Assert
        balance.Quantity.Should().Be(preciseQuantity);
    }

    [Fact]
    public void Balance_ShouldHandleMaxAndMinDecimalValues()
    {
        // Arrange & Act
        var balanceMax = new Balance { Quantity = decimal.MaxValue };
        var balanceMin = new Balance { Quantity = decimal.MinValue };

        // Assert
        balanceMax.Quantity.Should().Be(decimal.MaxValue);
        balanceMin.Quantity.Should().Be(decimal.MinValue);
    }

    [Fact]
    public void Balance_WithNavigationProperties_ShouldMaintainReferences()
    {
        // Arrange
        var balance = new Balance();
        var resource = TestDataBuilder.CreateValidResource("Test Resource");
        var unit = TestDataBuilder.CreateValidUnit("kg");

        // Act
        balance.Resource = resource;
        balance.Unit = unit;
        balance.ResourceId = resource.Id;
        balance.UnitId = unit.Id;

        // Assert
        balance.Resource.Should().BeSameAs(resource);
        balance.Unit.Should().BeSameAs(unit);
        balance.ResourceId.Should().Be(resource.Id);
        balance.UnitId.Should().Be(unit.Id);
    }

    [Theory]
    [InlineData(100.5, 25.25, 125.75)]
    [InlineData(50.0, -20.0, 30.0)]
    [InlineData(0.0, 100.0, 100.0)]
    [InlineData(10.5, -10.5, 0.0)]
    public void Balance_QuantityCalculations_ShouldBeAccurate(decimal initial, decimal change, decimal expected)
    {
        // Arrange
        var balance = new Balance { Quantity = initial };

        // Act
        balance.Quantity += change;

        // Assert
        balance.Quantity.Should().Be(expected);
    }

    [Fact]
    public void Balance_ShouldSupportBulkPropertyAssignment()
    {
        // Arrange
        var testData = new
        {
            Id = 123,
            ResourceId = 456,
            UnitId = 789,
            Quantity = 99.99m
        };

        // Act
        var balance = new Balance
        {
            Id = testData.Id,
            ResourceId = testData.ResourceId,
            UnitId = testData.UnitId,
            Quantity = testData.Quantity
        };

        // Assert
        balance.Id.Should().Be(testData.Id);
        balance.ResourceId.Should().Be(testData.ResourceId);
        balance.UnitId.Should().Be(testData.UnitId);
        balance.Quantity.Should().Be(testData.Quantity);
    }

    [Fact]
    public void Balance_ShouldSupportQuantityRounding()
    {
        // Arrange
        var balance = new Balance();
        var roundedQuantity = Math.Round(123.456789m, 2);

        // Act
        balance.Quantity = roundedQuantity;

        // Assert
        balance.Quantity.Should().Be(123.46m);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(999, 888)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void Balance_ShouldAcceptValidForeignKeys(int resourceId, int unitId)
    {
        // Arrange & Act
        var balance = new Balance
        {
            ResourceId = resourceId,
            UnitId = unitId
        };

        // Assert
        balance.ResourceId.Should().Be(resourceId);
        balance.UnitId.Should().Be(unitId);
    }

    [Fact]
    public void Balance_StockLevelChecks_ShouldWorkCorrectly()
    {
        // Arrange
        var lowStockBalance = new Balance { Quantity = 5.0m };
        var normalStockBalance = new Balance { Quantity = 100.0m };
        var overstockBalance = new Balance { Quantity = 10000.0m };
        var threshold = 10.0m;

        // Act & Assert
        (lowStockBalance.Quantity < threshold).Should().BeTrue();
        (normalStockBalance.Quantity >= threshold).Should().BeTrue();
        (overstockBalance.Quantity > 1000.0m).Should().BeTrue();
    }
}
