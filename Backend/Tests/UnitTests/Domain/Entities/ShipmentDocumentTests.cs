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
}
