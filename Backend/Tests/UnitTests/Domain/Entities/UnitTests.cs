using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Models.Entities;
using Domain.Models.Interfaces;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class UnitTests
{
    private readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    [Fact]
    public void Unit_ShouldImplementIModel()
    {
        // Arrange & Act
        var unit = new Unit();

        // Assert
        unit.Should().BeAssignableTo<IModel>();
    }

    [Fact]
    public void Unit_ShouldImplementIArchivable()
    {
        // Arrange & Act
        var unit = new Unit();

        // Assert
        unit.Should().BeAssignableTo<IArchivable>();
    }

    [Fact]
    public void Unit_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var unit = new Unit();

        // Assert
        unit.Id.Should().Be(0);
        unit.Name.Should().BeNull();
        unit.IsArchived.Should().BeFalse();
        unit.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        unit.UpdatedAt.Should().BeNull();
        unit.ReceiptItems.Should().BeNull();
        unit.ShipmentItems.Should().BeNull();
        unit.Balances.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void Unit_ShouldSetPropertiesCorrectly(int id, string name, bool isArchived)
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var unit = new Unit
        {
            Id = id,
            Name = name,
            IsArchived = isArchived,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        unit.Id.Should().Be(id);
        unit.Name.Should().Be(name);
        unit.IsArchived.Should().Be(isArchived);
        unit.CreatedAt.Should().Be(createdAt);
        unit.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Unit_CreatedAt_ShouldBeSetToUtcNow_WhenUsingDefaultConstructor()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var unit = new Unit();
        var afterCreation = DateTime.UtcNow;

        // Assert
        unit.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        unit.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Unit_ShouldSupportArchiving()
    {
        // Arrange
        var unit = _fixture.Create<Unit>();
        unit.IsArchived = false;

        // Act
        unit.IsArchived = true;

        // Assert
        unit.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Unit_ShouldSupportNavigationProperties()
    {
        // Arrange
        var unit = new Unit();
        var receiptItems = new List<ReceiptItem>();
        var shipmentItems = new List<ShipmentItem>();
        var balances = new List<Balance>();

        // Act
        unit.ReceiptItems = receiptItems;
        unit.ShipmentItems = shipmentItems;
        unit.Balances = balances;

        // Assert
        unit.ReceiptItems.Should().BeSameAs(receiptItems);
        unit.ShipmentItems.Should().BeSameAs(shipmentItems);
        unit.Balances.Should().BeSameAs(balances);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Unit_ShouldAcceptInvalidNameValues(string invalidName)
    {
        // Arrange & Act
        var unit = new Unit { Name = invalidName };

        // Assert
        // Note: The entity doesn't enforce validation at this level
        // Validation should be handled at the business logic layer
        unit.Name.Should().Be(invalidName);
    }

    [Fact]
    public void Unit_ShouldSupportUpdatedAtTracking()
    {
        // Arrange
        var unit = _fixture.Create<Unit>();
        var originalUpdatedAt = unit.UpdatedAt;
        var newUpdatedAt = DateTime.UtcNow;

        // Act
        unit.UpdatedAt = newUpdatedAt;

        // Assert
        unit.UpdatedAt.Should().Be(newUpdatedAt);
        unit.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    [Theory]
    [InlineData("kg")]
    [InlineData("piece")]
    [InlineData("liter")]
    [InlineData("meter")]
    public void Unit_ShouldAcceptValidUnitNames(string unitName)
    {
        // Arrange & Act
        var unit = new Unit { Name = unitName };

        // Assert
        unit.Name.Should().Be(unitName);
    }
}
