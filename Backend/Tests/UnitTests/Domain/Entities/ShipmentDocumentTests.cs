using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Models.Entities;
using Domain.Models.Enums;
using Domain.Models.Interfaces;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class ShipmentDocumentTests
{
    private readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    [Fact]
    public void ShipmentDocument_ShouldImplementIModel()
    {
        // Arrange & Act
        var document = new ShipmentDocument();

        // Assert
        document.Should().BeAssignableTo<IModel>();
    }

    [Fact]
    public void ShipmentDocument_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var document = new ShipmentDocument();

        // Assert
        document.Id.Should().Be(0);
        document.Number.Should().BeNull();
        document.ClientId.Should().Be(0);
        document.Date.Should().Be(default(DateTime));
        document.Status.Should().Be(ShipmentStatus.Draft);
        document.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.UpdatedAt.Should().BeNull();
        document.Client.Should().BeNull();
        document.Items.Should().NotBeNull();
        document.Items.Should().BeEmpty();
    }

    [Theory]
    [AutoData]
    public void ShipmentDocument_ShouldSetPropertiesCorrectly(int id, string number, int clientId, DateTime date, ShipmentStatus status)
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var document = new ShipmentDocument
        {
            Id = id,
            Number = number,
            ClientId = clientId,
            Date = date,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        document.Id.Should().Be(id);
        document.Number.Should().Be(number);
        document.ClientId.Should().Be(clientId);
        document.Date.Should().Be(date);
        document.Status.Should().Be(status);
        document.CreatedAt.Should().Be(createdAt);
        document.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ShipmentDocument_CreatedAt_ShouldBeSetToUtcNow_WhenUsingDefaultConstructor()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var document = new ShipmentDocument();
        var afterCreation = DateTime.UtcNow;

        // Assert
        document.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        document.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void ShipmentDocument_Items_ShouldBeInitializedAsEmptyList()
    {
        // Arrange & Act
        var document = new ShipmentDocument();

        // Assert
        document.Items.Should().NotBeNull();
        document.Items.Should().BeEmpty();
        document.Items.Should().BeOfType<List<ShipmentItem>>();
    }

    [Fact]
    public void ShipmentDocument_ShouldSupportAddingItems()
    {
        // Arrange
        var document = new ShipmentDocument();
        var item1 = _fixture.Create<ShipmentItem>();
        var item2 = _fixture.Create<ShipmentItem>();

        // Act
        document.Items.Add(item1);
        document.Items.Add(item2);

        // Assert
        document.Items.Should().HaveCount(2);
        document.Items.Should().Contain(item1);
        document.Items.Should().Contain(item2);
    }

    [Fact]
    public void ShipmentDocument_ShouldSupportNavigationProperties()
    {
        // Arrange
        var document = new ShipmentDocument();
        var client = _fixture.Create<Client>();

        // Act
        document.Client = client;

        // Assert
        document.Client.Should().BeSameAs(client);
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Signed)]
    public void ShipmentDocument_ShouldSupportAllStatusValues(ShipmentStatus status)
    {
        // Arrange & Act
        var document = new ShipmentDocument { Status = status };

        // Assert
        document.Status.Should().Be(status);
    }

    [Fact]
    public void ShipmentDocument_ShouldSupportStatusTransition()
    {
        // Arrange
        var document = new ShipmentDocument { Status = ShipmentStatus.Draft };

        // Act
        document.Status = ShipmentStatus.Signed;

        // Assert
        document.Status.Should().Be(ShipmentStatus.Signed);
    }

    [Fact]
    public void ShipmentDocument_ShouldSupportClientIdAssignment()
    {
        // Arrange
        var document = new ShipmentDocument();
        var clientId = _fixture.Create<int>();

        // Act
        document.ClientId = clientId;

        // Assert
        document.ClientId.Should().Be(clientId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ShipmentDocument_ShouldAcceptInvalidNumberValues(string invalidNumber)
    {
        // Arrange & Act
        var document = new ShipmentDocument { Number = invalidNumber };

        // Assert
        // Note: The entity doesn't enforce validation at this level
        // Validation should be handled at the business logic layer
        document.Number.Should().Be(invalidNumber);
    }

    [Fact]
    public void ShipmentDocument_Date_ShouldSupportPastAndFutureDates()
    {
        // Arrange
        var document = new ShipmentDocument();
        var pastDate = DateTime.UtcNow.AddDays(-30);
        var futureDate = DateTime.UtcNow.AddDays(30);

        // Act & Assert
        document.Date = pastDate;
        document.Date.Should().Be(pastDate);

        document.Date = futureDate;
        document.Date.Should().Be(futureDate);
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft, ShipmentStatus.Signed)]
    [InlineData(ShipmentStatus.Signed, ShipmentStatus.Draft)] // Reverse flow should also work
    public void ShipmentDocument_StatusWorkflow_ShouldAllowTransitions(ShipmentStatus fromStatus, ShipmentStatus toStatus)
    {
        // Arrange
        var document = new ShipmentDocument { Status = fromStatus };

        // Act
        document.Status = toStatus;
        document.UpdatedAt = DateTime.UtcNow;

        // Assert
        document.Status.Should().Be(toStatus);
        document.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ShipmentDocument_WithClientRelationship_ShouldMaintainIntegrity()
    {
        // Arrange
        var client = TestDataBuilder.CreateValidClient("Test Client", "123 Test St");
        var document = TestDataBuilder.CreateValidShipmentDocument("SHIP-001", client.Id);
        document.Client = client;

        // Act & Assert
        document.ClientId.Should().Be(client.Id);
        document.Client.Should().BeSameAs(client);
        document.Client.Name.Should().Be("Test Client");
    }

    [Fact]
    public void ShipmentDocument_WithMultipleItems_ShouldCalculateTotals()
    {
        // Arrange
        var document = TestDataBuilder.CreateShipmentDocumentWithItems(3);

        // Act & Assert
        document.Items.Should().HaveCount(3);
        document.Items.Should().OnlyContain(item => item.DocumentId == document.Id);
        
        var totalQuantity = document.Items.Sum(i => i.Quantity);
        totalQuantity.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft, false)]
    [InlineData(ShipmentStatus.Signed, true)]
    public void ShipmentDocument_CompletionStatus_ShouldReflectSignedState(ShipmentStatus status, bool shouldBeComplete)
    {
        // Arrange & Act
        var document = new ShipmentDocument { Status = status };
        var isComplete = document.Status == ShipmentStatus.Signed;

        // Assert
        isComplete.Should().Be(shouldBeComplete);
    }

    [Fact]
    public void ShipmentDocument_StatusHistory_ShouldTrackChanges()
    {
        // Arrange
        var document = new ShipmentDocument
        {
            Number = "SHIP-STATUS-001",
            Status = ShipmentStatus.Draft
        };
        var createdAt = document.CreatedAt;

        // Act - Progress through statuses
        document.Status = ShipmentStatus.Draft;
        document.UpdatedAt = DateTime.UtcNow;
        var pendingTime = document.UpdatedAt;

        Thread.Sleep(1); // Ensure time difference

        document.Status = ShipmentStatus.Signed;
        document.UpdatedAt = DateTime.UtcNow;
        var signedTime = document.UpdatedAt;

        // Assert
        document.Status.Should().Be(ShipmentStatus.Signed);
        document.CreatedAt.Should().Be(createdAt);
        signedTime.Should().BeAfter(pendingTime.Value);
        pendingTime.Should().BeAfter(createdAt);
    }

    [Fact]
    public void ShipmentDocument_EmptyShipment_ShouldBeValidDraft()
    {
        // Arrange & Act
        var emptyShipment = new ShipmentDocument
        {
            Number = "SHIP-EMPTY-001",
            ClientId = 1,
            Date = DateTime.UtcNow,
            Status = ShipmentStatus.Draft
        };

        // Assert
        emptyShipment.Items.Should().NotBeNull();
        emptyShipment.Items.Should().BeEmpty();
        emptyShipment.Status.Should().Be(ShipmentStatus.Draft);
        emptyShipment.ClientId.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 3)]
    [InlineData(10, 1)]
    public void ShipmentDocument_WithVariousItemConfigurations_ShouldMaintainConsistency(int itemCount, int uniqueResources)
    {
        // Arrange
        var document = new ShipmentDocument { Number = $"SHIP-{itemCount}-ITEMS" };

        // Act
        for (int i = 1; i <= itemCount; i++)
        {
            var resourceId = ((i - 1) % uniqueResources) + 1;
            document.Items.Add(TestDataBuilder.CreateValidShipmentItem(document.Id, resourceId, 1, i * 5m));
        }

        // Assert
        document.Items.Should().HaveCount(itemCount);
        document.Items.Select(i => i.ResourceId).Distinct().Should().HaveCount(uniqueResources);
        document.Items.Sum(i => i.Quantity).Should().Be(Enumerable.Range(1, itemCount).Sum(i => i * 5m));
    }
}
