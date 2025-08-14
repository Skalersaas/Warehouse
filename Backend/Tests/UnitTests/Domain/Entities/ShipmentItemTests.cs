using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Models.Entities;
using Domain.Models.Interfaces;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class ShipmentItemTests
{
    private readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    [Fact]
    public void ShipmentItem_ShouldImplementIModel()
    {
        // Arrange & Act
        var item = new ShipmentItem();

        // Assert
        item.Should().BeAssignableTo<IModel>();
    }

    [Fact]
    public void ShipmentItem_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var item = new ShipmentItem();

        // Assert
        item.Id.Should().Be(0);
        item.DocumentId.Should().Be(0);
        item.ResourceId.Should().Be(0);
        item.UnitId.Should().Be(0);
        item.Quantity.Should().Be(0);
        item.Document.Should().BeNull();
        item.Resource.Should().BeNull();
        item.Unit.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void ShipmentItem_ShouldSetPropertiesCorrectly(int id, int documentId, int resourceId, int unitId, decimal quantity)
    {
        // Arrange & Act
        var item = new ShipmentItem
        {
            Id = id,
            DocumentId = documentId,
            ResourceId = resourceId,
            UnitId = unitId,
            Quantity = quantity
        };

        // Assert
        item.Id.Should().Be(id);
        item.DocumentId.Should().Be(documentId);
        item.ResourceId.Should().Be(resourceId);
        item.UnitId.Should().Be(unitId);
        item.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void ShipmentItem_ShouldSupportNavigationProperties()
    {
        // Arrange
        var item = new ShipmentItem();
        var document = _fixture.Create<ShipmentDocument>();
        var resource = _fixture.Create<Resource>();
        var unit = _fixture.Create<Unit>();

        // Act
        item.Document = document;
        item.Resource = resource;
        item.Unit = unit;

        // Assert
        item.Document.Should().BeSameAs(document);
        item.Resource.Should().BeSameAs(resource);
        item.Unit.Should().BeSameAs(unit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void ShipmentItem_ShouldAcceptZeroAndNegativeQuantities(decimal quantity)
    {
        // Arrange & Act
        var item = new ShipmentItem { Quantity = quantity };

        // Assert
        // Note: Business rules about quantity validation should be handled at the service layer
        item.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(1.5)]
    [InlineData(999999.999)]
    public void ShipmentItem_ShouldAcceptPositiveQuantities(decimal quantity)
    {
        // Arrange & Act
        var item = new ShipmentItem { Quantity = quantity };

        // Assert
        item.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void ShipmentItem_ShouldSupportForeignKeyAssignment()
    {
        // Arrange
        var item = new ShipmentItem();
        var documentId = _fixture.Create<int>();
        var resourceId = _fixture.Create<int>();
        var unitId = _fixture.Create<int>();

        // Act
        item.DocumentId = documentId;
        item.ResourceId = resourceId;
        item.UnitId = unitId;

        // Assert
        item.DocumentId.Should().Be(documentId);
        item.ResourceId.Should().Be(resourceId);
        item.UnitId.Should().Be(unitId);
    }

    [Fact]
    public void ShipmentItem_ShouldAllowZeroForeignKeys()
    {
        // Arrange & Act
        var item = new ShipmentItem
        {
            DocumentId = 0,
            ResourceId = 0,
            UnitId = 0
        };

        // Assert
        // Note: Foreign key validation should be handled at the database/business logic level
        item.DocumentId.Should().Be(0);
        item.ResourceId.Should().Be(0);
        item.UnitId.Should().Be(0);
    }

    [Fact]
    public void ShipmentItem_ShouldSupportPreciseDecimalQuantities()
    {
        // Arrange
        var preciseQuantity = 123.456789m;

        // Act
        var item = new ShipmentItem { Quantity = preciseQuantity };

        // Assert
        item.Quantity.Should().Be(preciseQuantity);
    }

    [Fact]
    public void ShipmentItem_ShouldHandleMaxAndMinDecimalValues()
    {
        // Arrange & Act
        var itemMax = new ShipmentItem { Quantity = decimal.MaxValue };
        var itemMin = new ShipmentItem { Quantity = decimal.MinValue };

        // Assert
        itemMax.Quantity.Should().Be(decimal.MaxValue);
        itemMin.Quantity.Should().Be(decimal.MinValue);
    }
}
