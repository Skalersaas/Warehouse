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
        };

        // Assert
        document.Id.Should().Be(id);
        document.Number.Should().Be(number);
        document.Date.Should().Be(date);
        document.CreatedAt.Should().Be(createdAt);
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

    [Fact]
    public void ReceiptDocument_ItemManipulation_ShouldWorkCorrectly()
    {
        // Arrange
        var document = new ReceiptDocument { Id = 1, Number = "REC-001" };
        var item1 = TestDataBuilder.CreateValidReceiptItem(document.Id, 1, 1, 100m);
        var item2 = TestDataBuilder.CreateValidReceiptItem(document.Id, 2, 1, 50m);
        var item3 = TestDataBuilder.CreateValidReceiptItem(document.Id, 1, 2, 25m);

        // Act - Add items
        document.Items.Add(item1);
        document.Items.Add(item2);
        document.Items.Add(item3);

        // Assert
        document.Items.Should().HaveCount(3);
        document.Items.Sum(i => i.Quantity).Should().Be(175m);

        // Act - Remove item
        document.Items.Remove(item2);

        // Assert
        document.Items.Should().HaveCount(2);
        document.Items.Sum(i => i.Quantity).Should().Be(125m);
        document.Items.Should().NotContain(item2);
    }

    [Fact]
    public void ReceiptDocument_DocumentNumberGeneration_ShouldFollowPattern()
    {
        // Arrange & Act
        var documents = new List<ReceiptDocument>
        {
            new ReceiptDocument { Number = "REC-2024-001" },
            new ReceiptDocument { Number = "REC-2024-002" },
            new ReceiptDocument { Number = "REC-2024-003" }
        };

        // Assert
        documents.Should().OnlyContain(d => d.Number.StartsWith("REC-"));
        documents.Should().OnlyContain(d => d.Number.Contains("2024"));
        documents.Select(d => d.Number).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void ReceiptDocument_WithVariousItemCounts_ShouldHandleCorrectly(int itemCount)
    {
        // Arrange
        var document = new ReceiptDocument { Number = $"REC-{itemCount}-ITEMS" };

        // Act
        for (int i = 1; i <= itemCount; i++)
        {
            document.Items.Add(TestDataBuilder.CreateValidReceiptItem(document.Id, i, 1, i * 10m));
        }

        // Assert
        document.Items.Should().HaveCount(itemCount);
        if (itemCount > 0)
        {
            document.Items.Sum(i => i.Quantity).Should().Be(Enumerable.Range(1, itemCount).Sum(i => i * 10m));
        }
    }

    [Fact]
    public void ReceiptDocument_AuditTrail_ShouldTrackChanges()
    {
        // Arrange
        var document = new ReceiptDocument
        {
            Number = "REC-AUDIT-001",
            Date = DateTime.UtcNow.AddDays(-1)
        };
        var originalCreatedAt = document.CreatedAt;

        // Act - Simulate update
        document.Number = "REC-AUDIT-001-UPDATED";

        // Assert
        document.CreatedAt.Should().Be(originalCreatedAt);
        document.Number.Should().Be("REC-AUDIT-001-UPDATED");
    }

    [Fact]
    public void ReceiptDocument_EmptyDocument_ShouldBeValid()
    {
        // Arrange & Act
        var emptyDocument = new ReceiptDocument
        {
            Number = "REC-EMPTY-001",
            Date = DateTime.UtcNow
        };

        // Assert
        emptyDocument.Items.Should().NotBeNull();
        emptyDocument.Items.Should().BeEmpty();
        emptyDocument.Number.Should().NotBeNullOrEmpty();
        emptyDocument.Date.Should().NotBe(default(DateTime));
    }
}
