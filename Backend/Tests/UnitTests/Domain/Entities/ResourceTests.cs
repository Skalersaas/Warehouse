using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Models.Entities;
using Domain.Models.Interfaces;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class ResourceTests
{
    private readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    [Fact]
    public void Resource_ShouldImplementIModel()
    {
        // Arrange & Act
        var resource = new Resource();

        // Assert
        resource.Should().BeAssignableTo<IModel>();
    }

    [Fact]
    public void Resource_ShouldImplementIArchivable()
    {
        // Arrange & Act
        var resource = new Resource();

        // Assert
        resource.Should().BeAssignableTo<IArchivable>();
    }

    [Fact]
    public void Resource_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var resource = new Resource();

        // Assert
        resource.Id.Should().Be(0);
        resource.Name.Should().BeNull();
        resource.IsArchived.Should().BeFalse();
        resource.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        resource.ReceiptItems.Should().BeNull();
        resource.ShipmentItems.Should().BeNull();
        resource.Balances.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void Resource_ShouldSetPropertiesCorrectly(int id, string name, bool isArchived)
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var resource = new Resource
        {
            Id = id,
            Name = name,
            IsArchived = isArchived,
            CreatedAt = createdAt,
        };

        // Assert
        resource.Id.Should().Be(id);
        resource.Name.Should().Be(name);
        resource.IsArchived.Should().Be(isArchived);
        resource.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Resource_CreatedAt_ShouldBeSetToUtcNow_WhenUsingDefaultConstructor()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var resource = new Resource();
        var afterCreation = DateTime.UtcNow;

        // Assert
        resource.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        resource.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Resource_ShouldSupportArchiving()
    {
        // Arrange
        var resource = _fixture.Create<Resource>();
        resource.IsArchived = false;

        // Act
        resource.IsArchived = true;

        // Assert
        resource.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Resource_ShouldSupportNavigationProperties()
    {
        // Arrange
        var resource = new Resource();
        var receiptItems = new List<ReceiptItem>();
        var shipmentItems = new List<ShipmentItem>();
        var balances = new List<Balance>();

        // Act
        resource.ReceiptItems = receiptItems;
        resource.ShipmentItems = shipmentItems;
        resource.Balances = balances;

        // Assert
        resource.ReceiptItems.Should().BeSameAs(receiptItems);
        resource.ShipmentItems.Should().BeSameAs(shipmentItems);
        resource.Balances.Should().BeSameAs(balances);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Resource_ShouldAcceptInvalidNameValues(string invalidName)
    {
        // Arrange & Act
        var resource = new Resource { Name = invalidName };

        // Assert
        // Note: The entity doesn't enforce validation at this level
        // Validation should be handled at the business logic layer
        resource.Name.Should().Be(invalidName);
    }

}
