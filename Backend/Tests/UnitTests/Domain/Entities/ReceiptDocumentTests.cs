using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Models.Entities;
using Domain.Models.Interfaces;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class ReceiptDocumentTests
{
    private readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    [Fact]
    public void ReceiptDocument_ShouldImplementIModel()
    {
        // Arrange & Act
        var document = new ReceiptDocument();

        // Assert
        document.Should().BeAssignableTo<IModel>();
    }

    [Fact]
    public void ReceiptDocument_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var document = new ReceiptDocument();

        // Assert
        document.Id.Should().Be(0);
        document.Number.Should().BeNull();
        document.Date.Should().Be(default(DateTime));
        document.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.UpdatedAt.Should().BeNull();
        document.Items.Should().NotBeNull();
        document.Items.Should().BeEmpty();
    }

    [Theory]
    [AutoData]
    public void ReceiptDocument_ShouldSetPropertiesCorrectly(int id, string number, DateTime date)
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var document = new ReceiptDocument
        {
            Id = id,
            Number = number,
            Date = date,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        document.Id.Should().Be(id);
        document.Number.Should().Be(number);
        document.Date.Should().Be(date);
        document.CreatedAt.Should().Be(createdAt);
        document.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ReceiptDocument_CreatedAt_ShouldBeSetToUtcNow_WhenUsingDefaultConstructor()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var document = new ReceiptDocument();
        var afterCreation = DateTime.UtcNow;

        // Assert
        document.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        document.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void ReceiptDocument_Items_ShouldBeInitializedAsEmptyList()
    {
        // Arrange & Act
        var document = new ReceiptDocument();

        // Assert
        document.Items.Should().NotBeNull();
        document.Items.Should().BeEmpty();
        document.Items.Should().BeOfType<List<ReceiptItem>>();
    }

    [Fact]
    public void ReceiptDocument_ShouldSupportAddingItems()
    {
        // Arrange
        var document = new ReceiptDocument();
        var item1 = _fixture.Create<ReceiptItem>();
        var item2 = _fixture.Create<ReceiptItem>();

        // Act
        document.Items.Add(item1);
        document.Items.Add(item2);

        // Assert
        document.Items.Should().HaveCount(2);
        document.Items.Should().Contain(item1);
        document.Items.Should().Contain(item2);
    }

    [Fact]
    public void ReceiptDocument_ShouldSupportReplacingItemsCollection()
    {
        // Arrange
        var document = new ReceiptDocument();
        var newItems = _fixture.CreateMany<ReceiptItem>(3).ToList();

        // Act
        document.Items = newItems;

        // Assert
        document.Items.Should().BeSameAs(newItems);
        document.Items.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ReceiptDocument_ShouldAcceptInvalidNumberValues(string invalidNumber)
    {
        // Arrange & Act
        var document = new ReceiptDocument { Number = invalidNumber };

        // Assert
        // Note: The entity doesn't enforce validation at this level
        // Validation should be handled at the business logic layer
        document.Number.Should().Be(invalidNumber);
    }

    [Fact]
    public void ReceiptDocument_ShouldSupportUpdatedAtTracking()
    {
        // Arrange
        var document = _fixture.Create<ReceiptDocument>();
        var originalUpdatedAt = document.UpdatedAt;
        var newUpdatedAt = DateTime.UtcNow;

        // Act
        document.UpdatedAt = newUpdatedAt;

        // Assert
        document.UpdatedAt.Should().Be(newUpdatedAt);
        document.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    [Fact]
    public void ReceiptDocument_Date_ShouldSupportPastAndFutureDates()
    {
        // Arrange
        var document = new ReceiptDocument();
        var pastDate = DateTime.UtcNow.AddDays(-30);
        var futureDate = DateTime.UtcNow.AddDays(30);

        // Act & Assert
        document.Date = pastDate;
        document.Date.Should().Be(pastDate);

        document.Date = futureDate;
        document.Date.Should().Be(futureDate);
    }
}
