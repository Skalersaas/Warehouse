using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Models.Entities;
using Domain.Models.Interfaces;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class ReceiptItemTests
{
    private readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    [Fact]
    public void ReceiptItem_ShouldImplementIModel()
    {
        // Arrange & Act
        var item = new ReceiptItem();

        // Assert
        item.Should().BeAssignableTo<IModel>();
    }

    [Fact]
    public void ReceiptItem_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var item = new ReceiptItem();

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
    public void ReceiptItem_ShouldSetPropertiesCorrectly(int id, int documentId, int resourceId, int unitId, decimal quantity)
    {
        // Arrange & Act
        var item = new ReceiptItem
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
    public void ReceiptItem_ShouldSupportNavigationProperties()
    {
        // Arrange
        var item = new ReceiptItem();
        var document = _fixture.Create<ReceiptDocument>();
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
    public void ReceiptItem_ShouldAcceptZeroAndNegativeQuantities(decimal quantity)
    {
        // Arrange & Act
        var item = new ReceiptItem { Quantity = quantity };

        // Assert
        // Note: Business rules about quantity validation should be handled at the service layer
        item.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(1.5)]
    [InlineData(999999.999)]
    public void ReceiptItem_ShouldAcceptPositiveQuantities(decimal quantity)
    {
        // Arrange & Act
        var item = new ReceiptItem { Quantity = quantity };

        // Assert
        item.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void ReceiptItem_ShouldSupportForeignKeyAssignment()
    {
        // Arrange
        var item = new ReceiptItem();
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
    public void ReceiptItem_ShouldAllowZeroForeignKeys()
    {
        // Arrange & Act
        var item = new ReceiptItem
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
}
