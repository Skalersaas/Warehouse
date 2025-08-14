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
}
